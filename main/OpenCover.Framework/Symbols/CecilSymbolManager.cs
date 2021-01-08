using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;
using log4net;
using Mono.Collections.Generic;
using File = OpenCover.Framework.Model.File;
using SequencePoint = OpenCover.Framework.Model.SequencePoint;

namespace OpenCover.Framework.Symbols
{
    internal static class CecilSymbolManageExtensions
    {
        public static MethodBody SafeGetMethodBody(this MethodDefinition methodDefinition)
        {
            try
            {
                if (methodDefinition.HasBody)
                {
                    return methodDefinition.Body;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
       
    }

    internal class CecilSymbolManager : ISymbolManager
    {
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private readonly ITrackedMethodStrategyManager _trackedMethodStrategyManager;
        private readonly ISymbolFileHelper _symbolFileHelper;
        private AssemblyDefinition _sourceAssembly;
        private readonly Dictionary<int, MethodDefinition> _methodMap = new Dictionary<int, MethodDefinition>(); 

        public CecilSymbolManager(ICommandLine commandLine, IFilter filter, ILog logger, 
            ITrackedMethodStrategyManager trackedMethodStrategyManager, ISymbolFileHelper symbolFileHelper)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
            _trackedMethodStrategyManager = trackedMethodStrategyManager;
            _symbolFileHelper = symbolFileHelper;
        }

        public string ModulePath { get; private set; }

        public string ModuleName { get; private set; }

        public void Initialise(string modulePath, string moduleName)
        {
            ModulePath = modulePath;
            ModuleName = moduleName;
        }

        private AssemblyDefinition SearchForSymbolsAndLoad()
        {
            AssemblyDefinition sourceAssembly = null;
            var provider = new DefaultSymbolReaderProvider(true);

            try
            {
                sourceAssembly = AssemblyDefinition.ReadAssembly(ModulePath);
                if (sourceAssembly != null)
                {
                    var symbolReader = provider
                        .GetSymbolReader(sourceAssembly.MainModule, sourceAssembly.MainModule.FileName);
                    if (symbolReader != null)
                    {
                        sourceAssembly.MainModule.ReadSymbols(symbolReader);
                        if (sourceAssembly.MainModule.HasSymbols)
                            return sourceAssembly;
                    }
                }
            }
            catch (Exception)
            {
            }

            foreach (var symbolFile in _symbolFileHelper.GetSymbolFileLocations(ModulePath, _commandLine))
            {
                try
                {
                    using (var stream = System.IO.File.Open(symbolFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var parameters = new ReaderParameters 
                        {
                            SymbolReaderProvider = provider,
                            ReadingMode = ReadingMode.Immediate,
                            ReadSymbols = true,
                            SymbolStream = stream,
                            ThrowIfSymbolsAreNotMatching = true

                        };
                        sourceAssembly = AssemblyDefinition.ReadAssembly(ModulePath, parameters);
                        if (sourceAssembly.MainModule.HasSymbols)
                            return sourceAssembly;
                    }
                }
                catch (Exception)
                {
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
                        _sourceAssembly = SearchForSymbolsAndLoad();
                    }
                    finally
                    {
                        Environment.CurrentDirectory = currentPath;
                    }
                    if (_sourceAssembly == null && _logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat($"Cannot instrument {ModulePath} as no PDB/MDB could be loaded");
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
            if (SourceAssembly == null) 
                return new Class[0];
            var classes = new List<Class>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;

            GetInstrumentableTypes(typeDefinitions, classes, _filter, ModuleName);
            return classes.ToArray();
        }

        private static void GetInstrumentableTypes(IEnumerable<TypeDefinition> typeDefinitions, List<Class> classes, IFilter filter, string assemblyName)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.IsEnum) 
                    continue;
                if (typeDefinition.IsInterface && typeDefinition.IsAbstract) 
                    continue;

                var @class = BuildClass(filter, assemblyName, typeDefinition);

                // only instrument types that are not structs and have instrumentable points
                if (!typeDefinition.IsValueType || @class.Files.Maybe(f => f.Length) > 0)
                    classes.Add(@class);

                if (typeDefinition.HasNestedTypes) 
                    GetInstrumentableTypes(typeDefinition.NestedTypes, classes, filter, assemblyName); 
            }                                                                                        
        }

        private static IEnumerable<Tuple<Instruction, Mono.Cecil.Cil.SequencePoint>> GetInstructionsWithSequencePoints(
            MethodDefinition methodDefinition)
        {
            if (!methodDefinition.HasBody || !methodDefinition.DebugInformation.HasSequencePoints) yield break;
            if (methodDefinition.SafeGetMethodBody() == null) yield break;

            using (var iter = methodDefinition.Body.Instructions.OrderBy(x => x.Offset).GetEnumerator())
            foreach (var sequencePoint in methodDefinition.DebugInformation.SequencePoints.OrderBy(x => x.Offset))
            {
                while (iter.MoveNext())
                {
                    if (iter.Current.Offset == sequencePoint.Offset)
                    {
                        yield return new Tuple<Instruction, Mono.Cecil.Cil.SequencePoint>(iter.Current, sequencePoint);
                        break;
                    }
                }
            }
        }

        private static Class BuildClass(IFilter filter, string assemblyName, TypeDefinition typeDefinition)
        {
            var @class = new Class {FullName = typeDefinition.FullName};
            if (!filter.InstrumentClass(assemblyName, @class.FullName))
            {
                @class.MarkAsSkipped(SkippedMethod.Filter);
            }
            else if (filter.ExcludeByAttribute(typeDefinition))
            {
                @class.MarkAsSkipped(SkippedMethod.Attribute);
            }

            if (new[] {"System.MulticastDelegate", "System.Delegate"}.Contains(typeDefinition.BaseType?.FullName))
            {
                @class.MarkAsSkipped(SkippedMethod.Delegate);
            }

            var list = new List<string>();
            if (!@class.ShouldSerializeSkippedDueTo())
            {
                var files = from methodDefinition in typeDefinition.Methods
                    where methodDefinition.SafeGetMethodBody() != null && methodDefinition.Body.Instructions != null
                    from pts in GetInstructionsWithSequencePoints(methodDefinition)
                    select pts.Item2.Document.Url;

                list.AddRange(files.Distinct());
            }

            if (!typeDefinition.IsValueType || list.Count > 0)
                @class.Files = list.Distinct().Select(file => new File {FullPath = file}).ToArray();
            return @class;
        }

        public Method[] GetMethodsForType(Class type, File[] files)
        {
            var methods = new List<Method>();
            if (!type.ShouldSerializeSkippedDueTo())
            {
                IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
                GetMethodsForType(typeDefinitions, type.FullName, methods, files, _filter, _commandLine);
            }
            return methods.ToArray();
        }

        private static string GetFirstFile(MethodDefinition methodDefinition)
        {
            if (methodDefinition.SafeGetMethodBody() != null && methodDefinition.Body.Instructions != null)
            {
                var filePath = GetInstructionsWithSequencePoints(methodDefinition)
                    .FirstOrDefault(x => x.Item2.Document != null && !x.Item2.IsHidden)
                    .Maybe(x => x.Item2.Document.Url);
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
                if (methodDefinition.IsAbstract || methodDefinition.IsGetter || methodDefinition.IsSetter) 
                    continue;

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
                FullName = methodDefinition.FullName,
                IsConstructor = methodDefinition.IsConstructor,
                IsStatic = methodDefinition.IsStatic,
                IsGetter = methodDefinition.IsGetter,
                IsSetter = methodDefinition.IsSetter,
                MetadataToken = methodDefinition.MetadataToken.ToInt32()
            };

            if (methodDefinition.SafeGetMethodBody() == null)
            {
                method.MarkAsSkipped(methodDefinition.IsNative ? SkippedMethod.NativeCode : SkippedMethod.Unknown);
                return method;
            }

            if (alreadySkippedDueToAttr || filter.ExcludeByAttribute(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.Attribute);
            else if (filter.ExcludeByFile(GetFirstFile(methodDefinition)))
                method.MarkAsSkipped(SkippedMethod.File);
            else if (commandLine.SkipAutoImplementedProperties && filter.IsAutoImplementedProperty(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.AutoImplementedProperty);
            else if (filter.IsFSharpInternal(methodDefinition))
                method.MarkAsSkipped(SkippedMethod.FSharpInternal);

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
            if (_methodMap.Count > 0) 
                return;
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            BuildMethodMap(typeDefinitions);
        }

        private void BuildMethodMap(IEnumerable<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.SafeGetMethodBody() != null && methodDefinition.Body.Instructions != null))
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
            if (methodDefinition == null) 
                return;
            try
            {
                UInt32 ordinal = 0;
                list.AddRange(from x in GetInstructionsWithSequencePoints(methodDefinition)
                    where !x.Item2.IsHidden
                    let sp = x.Item2
                    select new SequencePoint
                    {
                        EndColumn = sp.EndColumn,
                        EndLine = sp.EndLine,
                        Offset = sp.Offset,
                        Ordinal = ordinal++,
                        StartColumn = sp.StartColumn,
                        StartLine = sp.StartLine,
                        Document = sp.Document.Url,
                    });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred with 'GetSequencePointsForToken' for method '{methodDefinition.FullName}'", ex);
            }
        }

        private IList<ICollection<Instruction>> GetInstrumentedBlocks(MethodDefinition methodDefinition)
        {
            // get a list of instructions that are covered by the sequence points
            var safeMethodBody = methodDefinition.SafeGetMethodBody();
            if (safeMethodBody == null)
                return null;
            var instructions = safeMethodBody.Instructions;

            var list = new List<ICollection<Instruction>>();

            ICollection<Instruction> collection = null;
            foreach (var instruction in instructions)
            {
                var sequencePoint = methodDefinition.DebugInformation
                    .GetSequencePoint(instruction);

                if (sequencePoint != null)
                {
                    if (collection != null)
                        list.Add(collection);

                    collection = sequencePoint.IsHidden ? null : new Collection<Instruction>();
                }

                if (collection != null)
                    collection.Add(instruction);
            }

            if (collection != null)
                list.Add(collection);

            return list;
        }

        private void GetBranchPointsForToken(int token, List<BranchPoint> list)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) 
                return;
            try
            {
                UInt32 ordinal = 0;
                var safeMethodBody = methodDefinition.SafeGetMethodBody();
                if (safeMethodBody == null) 
                    return;
                var instructions = safeMethodBody.Instructions;

                var instrumentedInstructions = GetInstrumentedBlocks(methodDefinition)
                    .SelectMany(block => block.Select(instruction => instruction))
                    .ToList();

                foreach (var instruction in instructions.Where(instruction => instruction.OpCode.FlowControl == FlowControl.Cond_Branch))
                {
                    if (!instrumentedInstructions.Contains(instruction))
                    {
                        if (instruction.Operand is Instruction jump && !instrumentedInstructions.Contains(jump))
                            continue;

                        if (instruction.Operand is Instruction[] jumps)
                        {
                            var contains = jumps.Any(jmp => instrumentedInstructions.Contains(jmp));
                            if (!contains)
                                continue;
                        }
                    }
                      
                    var pathCounter = 0;

                    // store branch origin offset
                    var branchOffset = instruction.Offset;
                    var closestSeqPt = FindClosestInstructionWithSequencePoint(methodDefinition.Body, instruction).Maybe(i => methodDefinition.DebugInformation.GetSequencePoint(i));
                    var branchingInstructionLine = closestSeqPt.Maybe(x => x.StartLine, -1);
                    var document = closestSeqPt.Maybe(x => x.Document.Url);

                    if (null == instruction.Next)
                        return;

                    if (!LoadPointsForConditionalBranch(list, instruction, branchingInstructionLine, document, branchOffset, pathCounter, instructions, ref ordinal)) 
                        return;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred with 'GetBranchPointsForToken' for method '{methodDefinition.FullName}'", ex);
            }
        }

        private bool LoadPointsForConditionalBranch(List<BranchPoint> list, Instruction instruction,
            int branchingInstructionLine, string document, int branchOffset, int pathCounter, 
            Collection<Instruction> instructions, ref uint ordinal)
        {
            // Add Conditional Branch (Path>=1)
            if (instruction.OpCode.Code == Code.Switch)
            {
                if (!(instruction.Operand is Instruction[] branchInstructions) || branchInstructions.Length == 0)
                    return false;

                ordinal = BuildPointsForConditionalBranch(list, instruction, branchInstructions, branchingInstructionLine,
                    document, branchOffset, ordinal, ref pathCounter);
            }
            else 
            {
                // Follow instruction at operand
                if (!(instruction.Operand is Instruction then))
                    return false;

                if (IgnoreConditionalBranchSequence(instruction, instructions, branchOffset))
                    return false;

                ordinal = BuildPointsForConditionalBranch(list, instruction, new[] { then }, branchingInstructionLine,
                    document, branchOffset, ordinal, ref pathCounter);
            }
            return true;
        }

        // some branches we just have to ignore
        private static readonly Regex CachedAnonymousDelegateFieldName = new Regex(@"^\<\>\d+__\d+_\d+$", RegexOptions.Compiled);
        private bool IgnoreConditionalBranchSequence(Instruction instruction, Collection<Instruction> instructions, int branchOffset)
        {
            var ignoreSequences = new[]
            {
                // new[]{ Code.Nop, Code.Nop, Code.Nop, },
                // we may need other samples
                new[] {Code.Brtrue_S, Code.Pop, Code.Ldsfld, Code.Ldftn, Code.Newobj, Code.Dup, Code.Stsfld}, // CachedAnonymousMethodDelegate field allocation 
            };

            if (ignoreSequences.Select(seq => seq.First()).Any(code => code == instruction.OpCode.Code))
            {
                var pathOffsetList = GetConditionalBranchPath(instruction.Next);
                var pathOffsetList1 = GetConditionalBranchPath(instruction.Operand as Instruction);

                var offsets = new[]
                {
                    branchOffset,
                    pathOffsetList.Last(),
                    pathOffsetList1.Last()
                };

                var bs = offsets.Min();
                var be = offsets.Max();

                var range = instructions.Where(i => (i.Offset >= bs) && (i.Offset <= be)).ToList();

                var match = ignoreSequences
                    .Where(ignoreSequence => range.Count >= ignoreSequence.Length)
                    .Any(ignoreSequence => range.Zip(ignoreSequence, (instr, code) => instr.OpCode.Code == code).All(x => x));

                if (match)
                {
                    // this is a final check on the field name
                    var inst = instruction.Previous?.Previous;
                    if (inst?.OpCode.Code == Code.Ldsfld)
                    {
                        var definition = inst.Operand as FieldReference;
                        var name = definition.Name;
                        return CachedAnonymousDelegateFieldName.Match(name).Success;
                    }
                }
            }

            return false;
        }

        private uint BuildPointsForConditionalBranch(List<BranchPoint> list, Instruction current, Instruction[] branchInstructions,
            int branchingInstructionLine, string document, int branchOffset, uint ordinal, ref int pathCounter)
        {
            // add Path 0
            var pathOffsetList = GetConditionalBranchPath(current.Next);
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

            var counter = pathCounter;
            // Add Conditional Branches (Path>0)
            list.AddRange(branchInstructions.Select(GetConditionalBranchPath)
                .Select(pathOffsetList1 => new BranchPoint
                {
                    StartLine = branchingInstructionLine,
                    Document = document,
                    Offset = branchOffset,
                    Ordinal = ordinal++,
                    Path = counter++,
                    OffsetPoints =
                        pathOffsetList1.Count > 1
                            ? pathOffsetList1.GetRange(0, pathOffsetList1.Count - 1)
                            : new List<int>(),
                    EndOffset = pathOffsetList1.Last()
                }));
            pathCounter = counter;
            return ordinal;
        }

        private List<int> GetConditionalBranchPath(Instruction instruction)
        {
            var offsetList = new List<int>();

            if (instruction != null)
            {
                var point = instruction;
                offsetList.Add(point.Offset);
                while (point.OpCode == OpCodes.Br || point.OpCode == OpCodes.Br_S)
                {
                    if (point.Operand is Instruction nextPoint)
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

        private static Instruction FindClosestInstructionWithSequencePoint(MethodBody methodBody, Instruction instruction)
        {
            var sequencePointsInMethod = methodBody.Instructions.Where(i => HasValidSequencePoint(i, methodBody.Method)).ToList();
            if (!sequencePointsInMethod.Any()) 
                return null;
            var idx = sequencePointsInMethod.BinarySearch(instruction, new InstructionByOffsetComparer());
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

        private static bool HasValidSequencePoint(Instruction instruction, MethodDefinition methodDefinition)
        {
            var sp = methodDefinition.DebugInformation.GetSequencePoint(instruction);
            return sp != null && !sp.IsHidden;
        }                                     

        private class InstructionByOffsetComparer : IComparer<Instruction>
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
            if (methodDefinition == null) 
                return;
            complexity = Gendarme.Rules.Maintainability.AvoidComplexMethodsRule.GetCyclomaticComplexity(methodDefinition);
        }

        public TrackedMethod[] GetTrackedMethods()
        {
            if (SourceAssembly==null) 
                return null;

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
