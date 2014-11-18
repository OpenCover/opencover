//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
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

        private SymbolFolder FindSymbolsFolder()
        {
            var origFolder = Path.GetDirectoryName(ModulePath);

            return FindSymbolsFolder(ModulePath, origFolder) ?? FindSymbolsFolder(ModulePath, _commandLine.TargetDir) ?? FindSymbolsFolder(ModulePath, Environment.CurrentDirectory);
        }

        private static SymbolFolder FindSymbolsFolder(string fileName, string targetfolder)
        {
            if (!string.IsNullOrEmpty(targetfolder) && Directory.Exists(targetfolder))
            {
                var name = Path.GetFileName(fileName);
                //Console.WriteLine(targetfolder);
                if (name != null)
                {
                    if (System.IO.File.Exists(Path.Combine(targetfolder, 
                        Path.GetFileNameWithoutExtension(fileName) + ".pdb")))
                    {
                        if (System.IO.File.Exists(Path.Combine(targetfolder, name)))
                            return new SymbolFolder(targetfolder, new PdbReaderProvider());   
                    }
                   
                    if (System.IO.File.Exists(Path.Combine(targetfolder, fileName + ".mdb")))
                    {
                        if (System.IO.File.Exists(Path.Combine(targetfolder, name)))
                            return new SymbolFolder(targetfolder, new MdbReaderProvider());
                    }
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
                        var symbolFolder = FindSymbolsFolder();
                        var folder = symbolFolder.Maybe(_ => _.TargetFolder) ?? Environment.CurrentDirectory;

                        var parameters = new ReaderParameters
                        {
                            SymbolReaderProvider = symbolFolder.SymbolReaderProvider ?? new PdbReaderProvider(),
                            ReadingMode = ReadingMode.Deferred,
                            ReadSymbols = true
                        };
                        var fileName = Path.GetFileName(ModulePath) ?? string.Empty;
                        _sourceAssembly = AssemblyDefinition.ReadAssembly(Path.Combine(folder, fileName), parameters);

                        if (_sourceAssembly != null)
                            _sourceAssembly.MainModule.ReadSymbols(parameters.SymbolReaderProvider.GetSymbolReader(_sourceAssembly.MainModule, _sourceAssembly.MainModule.FullyQualifiedName));
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
                            _logger.DebugFormat("Cannot instrument {0} as no PDB/MDB could be loaded", ModulePath);
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
                var @class = new Class { FullName = typeDefinition.FullName };
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
                    .FirstOrDefault(x => x.SequencePoint != null && x.SequencePoint.Document != null && x.SequencePoint.StartLine != StepOverLineCode)
                    .Maybe(x => x.SequencePoint.Document.Url);
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
            var method = new Method
            {
                Name = methodDefinition.FullName,
                IsConstructor = methodDefinition.IsConstructor,
                IsStatic = methodDefinition.IsStatic,
                IsGetter = methodDefinition.IsGetter,
                IsSetter = methodDefinition.IsSetter,
                MetadataToken = methodDefinition.MetadataToken.ToInt32()
            };

            if (alreadySkippedDueToAttr || filter.ExcludeByAttribute(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.Attribute);
            else if (filter.ExcludeByFile(GetFirstFile(methodDefinition)))
                method.MarkAsSkipped(SkippedMethod.File);
            else if (commandLine.SkipAutoImplementedProperties && filter.IsAutoImplementedProperty(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.AutoImplementedProperty);                

            var definition = methodDefinition;
            method.FileRef = files.Where(x => x.FullPath == GetFirstFile(definition))
                .Select(x => new FileRef {UniqueId = x.UniqueId}).FirstOrDefault();
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
            list.AddRange(from instruction in methodDefinition.Body.Instructions
                where instruction.SequencePoint != null && instruction.SequencePoint.StartLine != StepOverLineCode
                let sp = instruction.SequencePoint
                select new SequencePoint
                {
                    EndColumn = sp.EndColumn, EndLine = sp.EndLine, Offset = instruction.Offset, Ordinal = ordinal++, StartColumn = sp.StartColumn, StartLine = sp.StartLine, Document = sp.Document.Url,
                });
        }

        private void GetBranchPointsForToken(int token, List<BranchPoint> list)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) return;
            UInt32 ordinal = 0;

            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                if (BranchIsInGeneratedFinallyBlock(instruction, methodDefinition)) continue;

                var pathCounter = 0;

                // store branch origin offset
                var branchOffset = instruction.Offset;
                var closestSeqPt = FindClosestSequencePoints(methodDefinition.Body, instruction);
                var branchingInstructionLine = closestSeqPt.Maybe(sp => sp.SequencePoint.StartLine, -1);
                var document = closestSeqPt.Maybe(sp => sp.SequencePoint.Document.Url);

                if (null == instruction.Next)
                    return;

                // Add Default branch (Path=0)

                // Follow else/default instruction
                var @else = instruction.Next;

                var pathOffsetList = GetBranchPath(@else);

                // add Path 0
                var path0 = new BranchPoint
                {
                    StartLine = branchingInstructionLine,
                    Document = document,
                    Offset = branchOffset,
                    Ordinal = ordinal++,
                    Path = pathCounter++,
                    OffsetPoints =
                        pathOffsetList.Count > 1
                            ? pathOffsetList.GetRange(0, pathOffsetList.Count - 1)
                            : new List<int>(),
                    EndOffset = pathOffsetList.Last()
                };
                list.Add(path0);

                // Add Conditional Branch (Path=1)
                if (instruction.OpCode.Code != Code.Switch)
                {
                    // Follow instruction at operand
                    var @then = instruction.Operand as Instruction;
                    if (@then == null)
                        return;

                    pathOffsetList = GetBranchPath(@then);

                    // Add path 1
                    var path1 = new BranchPoint
                    {
                        StartLine = branchingInstructionLine,
                        Document = document,
                        Offset = branchOffset,
                        Ordinal = ordinal++,
                        Path = pathCounter,
                        OffsetPoints =
                            pathOffsetList.Count > 1
                                ? pathOffsetList.GetRange(0, pathOffsetList.Count - 1)
                                : new List<int>(),
                        EndOffset = pathOffsetList.Last()
                    };
                    list.Add(path1);
                }
                else // instruction.OpCode.Code == Code.Switch
                {
                    var branchInstructions = instruction.Operand as Instruction[];
                    if (branchInstructions == null || branchInstructions.Length == 0)
                        return;

                    // Add Conditional Branches (Path>0)
                    foreach (var @case in branchInstructions)
                    {
                        // Follow operand istruction
                        pathOffsetList = GetBranchPath(@case);
            
                        // add paths 1..n
                        var path1ToN = new BranchPoint
                        {
                            StartLine = branchingInstructionLine,
                            Document = document,
                            Offset = branchOffset,
                            Ordinal = ordinal++,
                            Path = pathCounter++,
                            OffsetPoints =
                                pathOffsetList.Count > 1
                                    ? pathOffsetList.GetRange(0, pathOffsetList.Count - 1)
                                    : new List<int>(),
                            EndOffset = pathOffsetList.Last()
                        };
                        list.Add(path1ToN);
                    }
                }
            }
        }

        private static bool BranchIsInGeneratedFinallyBlock(Instruction branchInstruction, MethodDefinition methodDefinition)
        {
            if (!methodDefinition.Body.HasExceptionHandlers) 
                return false;

            // a generated finally block will have no sequence points in its range
            return methodDefinition.Body.ExceptionHandlers
                .Where(e => e.HandlerType == ExceptionHandlerType.Finally)
                .Where(e => branchInstruction.Offset >= e.HandlerStart.Offset && branchInstruction.Offset < e.HandlerEnd.Offset)
                .OrderByDescending(h => h.HandlerStart.Offset) // we need to work inside out
                .Any(eh => !methodDefinition.Body.Instructions
                    .Where(i => i.SequencePoint != null && i.SequencePoint.StartLine != StepOverLineCode)
                    .Any(i => i.Offset >= eh.HandlerStart.Offset && i.Offset < eh.HandlerEnd.Offset));
        }

        private List<int> GetBranchPath(Instruction instruction)
        {
            var offsetList = new List<int>();

            if (instruction != null)
            {
                var point = instruction;
                offsetList.Add(point.Offset);
                while ( point.OpCode == OpCodes.Br || point.OpCode == OpCodes.Br_S )
                {
                    var nextPoint = point.Operand as Instruction;
                    if (nextPoint != null)
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

            return offsetList;
        }

        private Instruction FindClosestSequencePoints(MethodBody methodBody, Instruction instruction)
        {
            var sequencePointsInMethod = methodBody.Instructions.Where(HasValidSequencePoint).ToList();
            if (!sequencePointsInMethod.Any()) return null;
            var idx = sequencePointsInMethod.BinarySearch(instruction, new InstructionByOffsetCompararer());
            Instruction prev;
            if (idx < 0)
            {
                // no exact match, idx corresponds to the next, larger element
                var lower = Math.Max(~idx - 1, 0);
                prev = sequencePointsInMethod[lower];
            }
            else
            {
                // exact match, idx corresponds to the match
                prev = sequencePointsInMethod[idx];
            }

            return prev;
        }

        private bool HasValidSequencePoint(Instruction instruction)
        {
            return instruction.SequencePoint != null && instruction.SequencePoint.StartLine != StepOverLineCode;
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
                var fileName = Path.GetFileName(modulePath) ?? string.Empty;
                modulePath = Path.Combine(_commandLine.TargetDir, fileName);
            }

            return _trackedMethodStrategyManager.GetTrackedMethods(modulePath);
        }
    }
}
