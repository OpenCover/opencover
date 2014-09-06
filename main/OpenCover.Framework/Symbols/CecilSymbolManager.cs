//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;
using log4net;
using File = OpenCover.Framework.Model.File;
using SequencePoint = OpenCover.Framework.Model.SequencePoint;

namespace OpenCover.Framework.Symbols
{
    internal class CecilSymbolManager : ISymbolManager
    {
        private const int StepOverLineCode = 0xFEEFEE;
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private readonly ITrackedMethodStrategyManager _trackedMethodStrategyManager;
        private AssemblyDefinition _sourceAssembly;
        private readonly Dictionary<int, MethodDefinition> _methodMap = new Dictionary<int, MethodDefinition>(); 

        public CecilSymbolManager(ICommandLine commandLine, IFilter filter, ILog logger, ITrackedMethodStrategyManager trackedMethodStrategyManager)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
            _trackedMethodStrategyManager = trackedMethodStrategyManager;
        }

        public string ModulePath { get; private set; }

        public string ModuleName { get; private set; }

        public void Initialise(string modulePath, string moduleName)
        {
            ModulePath = modulePath;
            ModuleName = moduleName;
        }

        private string FindSymbolsFolder()
        {
            var origFolder = Path.GetDirectoryName(ModulePath);

            return FindSymbolsFolder(ModulePath, origFolder) ?? FindSymbolsFolder(ModulePath, _commandLine.TargetDir) ?? FindSymbolsFolder(ModulePath, Environment.CurrentDirectory);
        }

        private static string FindSymbolsFolder(string fileName, string targetfolder)
        {
            if (!string.IsNullOrEmpty(targetfolder) && Directory.Exists(targetfolder))
            {
                if (System.IO.File.Exists(Path.Combine(targetfolder, Path.GetFileNameWithoutExtension(fileName) + ".pdb")))
                {
                    if (System.IO.File.Exists(Path.Combine(targetfolder, Path.GetFileName(fileName))))
                        return targetfolder;   
                }
            }
            return null;
        }

        public AssemblyDefinition SourceAssembly
        {
            get
            {
                if (_sourceAssembly==null)
                {
                    var currentPath = Environment.CurrentDirectory;
                    try
                    {
                        var folder = FindSymbolsFolder();
                        folder = folder ?? Environment.CurrentDirectory;

                        var parameters = new ReaderParameters
                        {
                            SymbolReaderProvider = new PdbReaderProvider(),
                            ReadingMode = ReadingMode.Deferred,
                            ReadSymbols = true,
                        };
                        _sourceAssembly = AssemblyDefinition.ReadAssembly(Path.Combine(folder, Path.GetFileName(ModulePath)), parameters);

                        if (_sourceAssembly != null)
                            _sourceAssembly.MainModule.ReadSymbols();
                    }
                    catch (Exception)
                    {
                        // failure to here is quite normal for DLL's with no PDBs => no instrumentation
                        _sourceAssembly = null;
                    }
                    finally
                    {
                        Environment.CurrentDirectory = currentPath;
                    }
                    if (_sourceAssembly == null)
                    {
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugFormat("Cannot instrument {0} as no PDB could be loaded", ModulePath);
                        }
                    }
                }
                return _sourceAssembly;
            }
        }

        public File[] GetFiles()
        {
            var list = new List<File>();
            foreach (var instrumentableType in GetInstrumentableTypes())
            {
                list.AddRange(instrumentableType.Files);
            }
            return list.Distinct(new FileEqualityComparer()).Select(file => file).ToArray();
        }

        public Class[] GetInstrumentableTypes()
        {
            if (SourceAssembly == null) return new Class[0];
            var classes = new List<Class>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetInstrumentableTypes(typeDefinitions, classes, _filter, ModuleName);
            return classes.ToArray();
        }

        private static void GetInstrumentableTypes(IEnumerable<TypeDefinition> typeDefinitions, List<Class> classes, IFilter filter, string moduleName)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.IsEnum) continue;
                if (typeDefinition.IsInterface && typeDefinition.IsAbstract) continue;
                var @class = new Class() { FullName = typeDefinition.FullName };
                if (!filter.InstrumentClass(moduleName, @class.FullName))
                {
                    @class.MarkAsSkipped(SkippedMethod.Filter);
                }
                else if (filter.ExcludeByAttribute(typeDefinition))
                {
                    @class.MarkAsSkipped(SkippedMethod.Attribute);
                }

                var list = new List<string>();
                if (!@class.ShouldSerializeSkippedDueTo())
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.Body != null && methodDefinition.Body.Instructions != null)
                        {
                            foreach (var instruction in methodDefinition.Body.Instructions)
                            {
                                if (instruction.SequencePoint != null)
                                {
                                    list.Add(instruction.SequencePoint.Document.Url);
                                    break;
                                }
                            }
                        }
                    }
                }

                // only instrument types that are not structs and have instrumentable points
                if (!typeDefinition.IsValueType || list.Count > 0)
                {
                    @class.Files = list.Distinct().Select(file => new File { FullPath = file }).ToArray();
                    classes.Add(@class);
                }
                if (typeDefinition.HasNestedTypes) 
                    GetInstrumentableTypes(typeDefinition.NestedTypes, classes, filter, moduleName); 
            }                                                                                        
        }

       
        public Method[] GetMethodsForType(Class type, File[] files)
        {
            var methods = new List<Method>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetMethodsForType(typeDefinitions, type.FullName, methods, files, _filter, _commandLine);
            return methods.ToArray();
        }

        private static string GetFirstFile(MethodDefinition definition)
        {
            if (definition.HasBody && definition.Body.Instructions!=null)
            {
                var filePath = definition.Body.Instructions
                    .Where(x => x.SequencePoint != null && x.SequencePoint.Document != null && x.SequencePoint.StartLine != StepOverLineCode)
                    .Select(x => x.SequencePoint.Document.Url)
                    .FirstOrDefault();
                return filePath;
            }
            return null;
        }

        private static void GetMethodsForType(IEnumerable<TypeDefinition> typeDefinitions, string fullName, List<Method> methods, File[] files, IFilter filter,ICommandLine commandLine)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.FullName == fullName)
                {
                    BuildPropertyMethods(methods, files, filter, typeDefinition, commandLine);
                    BuildMethods(methods, files, filter, typeDefinition, commandLine);
                }
                if (typeDefinition.HasNestedTypes) 
                    GetMethodsForType(typeDefinition.NestedTypes, fullName, methods, files, filter, commandLine);
            }
        }

        private static void BuildMethods(ICollection<Method> methods, File[] files, IFilter filter, TypeDefinition typeDefinition, ICommandLine commandLine)
        {
            foreach (var methodDefinition in typeDefinition.Methods)
            {
                if (methodDefinition.IsAbstract) continue;
                if (methodDefinition.IsGetter) continue;
                if (methodDefinition.IsSetter) continue;

                var method = BuildMethod(files, filter, methodDefinition, false, commandLine);
                methods.Add(method);
            }
        }

        private static void BuildPropertyMethods(ICollection<Method> methods, File[] files, IFilter filter, TypeDefinition typeDefinition, ICommandLine commandLine)
        {
            foreach (var propertyDefinition in typeDefinition.Properties)
            {
                var skipped = filter.ExcludeByAttribute(propertyDefinition);
                
                if (propertyDefinition.GetMethod != null && !propertyDefinition.GetMethod.IsAbstract)
                {
                    var method = BuildMethod(files, filter, propertyDefinition.GetMethod, skipped, commandLine);
                    methods.Add(method);
                }

                if (propertyDefinition.SetMethod != null && !propertyDefinition.SetMethod.IsAbstract)
                {
                    var method = BuildMethod(files, filter, propertyDefinition.SetMethod, skipped, commandLine);
                    methods.Add(method);
                }
            }
        }

        private static Method BuildMethod(IEnumerable<File> files, IFilter filter, MethodDefinition methodDefinition, bool alreadySkippedDueToAttr, ICommandLine commandLine)
        {
            var method = new Method();
            method.Name = methodDefinition.FullName;
            method.IsConstructor = methodDefinition.IsConstructor;
            method.IsStatic = methodDefinition.IsStatic;
            method.IsGetter = methodDefinition.IsGetter;
            method.IsSetter = methodDefinition.IsSetter;
            method.MetadataToken = methodDefinition.MetadataToken.ToInt32();

            if (alreadySkippedDueToAttr || filter.ExcludeByAttribute(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.Attribute);
            else if (filter.ExcludeByFile(GetFirstFile(methodDefinition)))
                method.MarkAsSkipped(SkippedMethod.File);
            else if (commandLine.SkipAutoImplementedProperties && filter.IsAutoImplementedProperty(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.AutoImplementedProperty);                

            var definition = methodDefinition;
            method.FileRef = files.Where(x => x.FullPath == GetFirstFile(definition))
                .Select(x => new FileRef() {UniqueId = x.UniqueId}).FirstOrDefault();
            return method;
        }

        public SequencePoint[] GetSequencePointsForToken(int token)
        {
            BuildMethodMap();
            var list = new List<SequencePoint>();
            GetSequencePointsForToken(token, list);
            return list.ToArray();
        }

        public BranchPoint[] GetBranchPointsForToken(int token)
        {
            BuildMethodMap();
            var list = new List<BranchPoint>();
            GetBranchPointsForToken(token, list);
            return list.ToArray();
        }

        public int GetCyclomaticComplexityForToken(int token)
        {
            BuildMethodMap();
            var complexity = 0;
            GetCyclomaticComplexityForToken(token, ref complexity);
            return complexity;
        }

        // TODO: Build a map of tokens to method definitions
        private void BuildMethodMap()
        {
            if (_methodMap.Count > 0) return;
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            BuildMethodMap(typeDefinitions);
        }

        private void BuildMethodMap(IEnumerable<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.Body != null && methodDefinition.Body.Instructions != null))
                {
                    _methodMap.Add(methodDefinition.MetadataToken.ToInt32(), methodDefinition);
                }
                if (typeDefinition.HasNestedTypes)
                {
                    BuildMethodMap(typeDefinition.NestedTypes);
                }
            }
        }

        private void GetSequencePointsForToken(int token, List<SequencePoint> list)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) return;
            UInt32 ordinal = 0;
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.SequencePoint != null &&
                    instruction.SequencePoint.StartLine != StepOverLineCode)
                {
                    var sp = instruction.SequencePoint;
                    var point = new SequencePoint()
                                    {
                                        EndColumn = sp.EndColumn,
                                        EndLine = sp.EndLine,
                                        Offset = instruction.Offset,
                                        Ordinal = ordinal++,
                                        StartColumn = sp.StartColumn,
                                        StartLine = sp.StartLine,
                                        Document = sp.Document.Url,
                                    };
                    list.Add(point);
                }
            }
        }

        private void GetBranchPointsForToken(int token, List<BranchPoint> list)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) return;
            UInt32 ordinal = 0;
            int branchOffset = 0;
            int pathCounter = 0;
            List<int> PathOffsetList;

            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                pathCounter = 0;

                // store branch origin offset
                branchOffset = instruction.Offset;
                var branchingInstructionLine =
                    FindClosestSequencePoints(methodDefinition.Body, instruction)
                        .Maybe(sp => sp.SequencePoint.StartLine, -1);

                Debug.Assert(!Object.ReferenceEquals(null, instruction.Next));
                if (Object.ReferenceEquals(null, instruction.Next))
                {
                    return;
                }

                // Add Default branch (Path=0)
                Debug.Assert(pathCounter == 0);

                // Follow else/default instruction
                Instruction @else = instruction.Next;

                PathOffsetList = GetBranchPath(@else);
                Debug.Assert(PathOffsetList.Count > 0);

                // add Path 0
                BranchPoint path0 = new BranchPoint()
                {
                    StartLine = branchingInstructionLine,
                    Offset = branchOffset,
                    Ordinal = ordinal++,
                    Path = pathCounter++,
                    OffsetPoints =
                        PathOffsetList.Count > 1
                            ? PathOffsetList.GetRange(0, PathOffsetList.Count - 1)
                            : new List<int>(),
                    EndOffset = PathOffsetList.Last()
                };
                Debug.Assert(!Object.ReferenceEquals(null, path0.OffsetPoints));
                list.Add(path0);

                Debug.Assert(ordinal != 0);
                Debug.Assert(pathCounter != 0);

                // Add Conditional Branch (Path=1)
                if (instruction.OpCode.Code != Code.Switch)
                {
                    // Follow instruction at operand
                    Instruction @then = instruction.Operand as Instruction;
                    Debug.Assert(!Object.ReferenceEquals(null, @then));
                    if (Object.ReferenceEquals(null, @then))
                    {
                        return;
                    }

                    PathOffsetList = GetBranchPath(@then);
                    Debug.Assert(PathOffsetList.Count > 0);

                    // Add path 1
                    BranchPoint path1 = new BranchPoint()
                    {
                        StartLine = branchingInstructionLine,
                        Offset = branchOffset,
                        Ordinal = ordinal++,
                        Path = pathCounter++,
                        OffsetPoints =
                            PathOffsetList.Count > 1
                                ? PathOffsetList.GetRange(0, PathOffsetList.Count - 1)
                                : new List<int>(),
                        EndOffset = PathOffsetList.Last()
                    };
                    Debug.Assert(!Object.ReferenceEquals(null, path1.OffsetPoints));
                    list.Add(path1);
                }
                else // instruction.OpCode.Code == Code.Switch
                {
                    Instruction[] branchInstructions = instruction.Operand as Instruction[];

                    Debug.Assert(!Object.ReferenceEquals(null, branchInstructions));
                    Debug.Assert(branchInstructions.Length != 0);

                    if (Object.ReferenceEquals(null, branchInstructions) || branchInstructions.Length == 0)
                    {
                        return;
                    }

                    // Add Conditional Branches (Path>0)
                    foreach (var @case in branchInstructions)
                    {

                        // Follow operand istruction
                        PathOffsetList = GetBranchPath(@case);
                        Debug.Assert(PathOffsetList.Count > 0);

                        // add paths 1..n
                        BranchPoint path1toN = new BranchPoint()
                        {
                            StartLine = branchingInstructionLine,
                            Offset = branchOffset,
                            Ordinal = ordinal++,
                            Path = pathCounter++,
                            OffsetPoints =
                                PathOffsetList.Count > 1
                                    ? PathOffsetList.GetRange(0, PathOffsetList.Count - 1)
                                    : new List<int>(),
                            EndOffset = PathOffsetList.Last()
                        };
                        Debug.Assert(!Object.ReferenceEquals(null, path1toN.OffsetPoints));
                        list.Add(path1toN);
                    }
                }
            }
        }

        private List<int> GetBranchPath(Instruction instruction)
        {
            //Contract.Requires<ArgumentNullException>(!Object.ReferenceEquals(null, instruction));
            //Contract.Ensures(!Object.ReferenceEquals(null, Debug.Result<List<int>>()));
            //Contract.Ensures(Debug.Result<List<int>>().Count != 0, "Returned List<int>.Count == 0");

            Debug.Assert(!Object.ReferenceEquals(null, instruction));

            List<int> offsetList = new List<int>();

            if (!Object.ReferenceEquals(null, instruction) )
            {

                Instruction nextPoint = null;
                Instruction point = instruction;
                offsetList.Add(point.Offset);
                while ( point.OpCode == OpCodes.Br || point.OpCode == OpCodes.Br_S )
                {
                    nextPoint = point.Operand as Instruction;
                    Debug.Assert(!Object.ReferenceEquals(null, point));
                    if ( !Object.ReferenceEquals(null, nextPoint) )
                    {
                        point = nextPoint;
                        offsetList.Add(point.Offset);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Debug.Assert(!Object.ReferenceEquals(null, offsetList));
            Debug.Assert(offsetList.Count != 0, "Returned List<int>.Count == 0");
            return offsetList;
        }

        private Instruction FindClosestSequencePoints(MethodBody methodBody, Instruction instruction)
        {
            Debug.Assert(methodBody != null);
            Debug.Assert(methodBody.Instructions != null);

            var ignoreableSequencePoints = new HashSet<Instruction>();
            if (methodBody.HasExceptionHandlers)
            {
                ignoreableSequencePoints = new HashSet<Instruction>(
                    methodBody.ExceptionHandlers
                    .Where(eh => eh.HandlerType == ExceptionHandlerType.Finally)
                    .Select(eh => eh.TryEnd).Where(IsIgnoreableInstruction)); 
            }

            var sequencePointsInMethod = methodBody.Instructions.Where(i => i.SequencePoint != null).ToList();
            if (!sequencePointsInMethod.Any()) return null;
            var idx = sequencePointsInMethod.BinarySearch(instruction, new InstructionByOffsetCompararer());
            Instruction prev;
            if (idx < 0)
            {
                // no exact match, idx corresponds to the next, larger element
                idx = Math.Max(~idx - 1, 0);
            }

            prev = sequencePointsInMethod[idx];
            if (ignoreableSequencePoints.Contains(prev))
            {
                return null;
            }
            else if(IsIgnoreableInstruction(prev))
            {
                idx = Math.Max(idx - 1, 0);
                prev = sequencePointsInMethod[idx];
            }            

            Debug.Assert(prev.SequencePoint != null);
            return prev;
        }

        private bool IsIgnoreableInstruction(Instruction instruction)
        {
            return instruction.SequencePoint == null || instruction.SequencePoint.StartLine == StepOverLineCode;
        }

        private class InstructionByOffsetCompararer : IComparer<Instruction>
        {
            public int Compare(Instruction x, Instruction y)
            {
                return x.Offset.CompareTo(y.Offset);
            }
        }

        private MethodDefinition GetMethodDefinition(int token)
        {
            return !_methodMap.ContainsKey(token) ? null : _methodMap[token];
        }

        private void GetCyclomaticComplexityForToken(int token, ref int complexity)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) return;
            complexity = Gendarme.Rules.Maintainability.AvoidComplexMethodsRule.GetCyclomaticComplexity(methodDefinition);
        }

        public TrackedMethod[] GetTrackedMethods()
        {
            if (SourceAssembly==null) return null;

            var modulePath = ModulePath;
            if (!System.IO.File.Exists(modulePath))
            {
                modulePath = Path.Combine(_commandLine.TargetDir, Path.GetFileName(modulePath));
            }

            return _trackedMethodStrategyManager.GetTrackedMethods(modulePath);
        }
    }
}
