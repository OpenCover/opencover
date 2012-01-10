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
using log4net;
using File = OpenCover.Framework.Model.File;
using SequencePoint = OpenCover.Framework.Model.SequencePoint;

namespace OpenCover.Framework.Symbols
{
    internal class CecilSymbolManager : ISymbolManager
    {
        private const int stepOverLineCode = 0xFEEFEE;
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private string _modulePath;
        private string _moduleName;
        private AssemblyDefinition _sourceAssembly;

        public CecilSymbolManager(ICommandLine commandLine, IFilter filter, ILog logger)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
        }

        public string ModulePath
        {
            get { return _modulePath; }
        }

        public string ModuleName
        {
            get { return _moduleName; }
        }

        public void Initialise(string modulePath, string moduleName)
        {
            _modulePath = modulePath;
            _moduleName = moduleName;
        }

        private string FindSymbolsFolder()
        {
            var origFolder = Path.GetDirectoryName(_modulePath);

            return FindSymbolsFolder(_modulePath, origFolder) ?? FindSymbolsFolder(_modulePath, _commandLine.TargetDir) ?? FindSymbolsFolder(_modulePath, Environment.CurrentDirectory);
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
                        _sourceAssembly = AssemblyDefinition.ReadAssembly(Path.Combine(folder, Path.GetFileName(_modulePath)), parameters);

                        if (_sourceAssembly != null)
                            _sourceAssembly.MainModule.ReadSymbols();
                    }
                    catch (Exception ex)
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
                            _logger.DebugFormat("Cannot instrument {0} as no PDB could be loaded", _modulePath);
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
            GetInstrumentableTypes(typeDefinitions, classes, _filter);
            return classes.Where(c => _filter.InstrumentClass(_moduleName, c.FullName)).ToArray();
        }

        private static void GetInstrumentableTypes(IEnumerable<TypeDefinition> typeDefinitions, List<Class> classes, IFilter filter)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.IsEnum) continue;
                if (typeDefinition.IsInterface && typeDefinition.IsAbstract) continue;
                var @class = new Class() { FullName = typeDefinition.FullName };
                if (filter.ExcludeByAttribute(typeDefinition))
                {
                    @class.SkippedDueTo = SkippedMethod.Attribute;
                }
                var list = new List<string>();
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

                // only instrument types that are not structs and have instrumentable points
                if (!typeDefinition.IsValueType || list.Count > 0)
                {
                    @class.Files = list.Distinct().Select(file => new File { FullPath = file }).ToArray();
                    classes.Add(@class);
                }
                if (typeDefinition.HasNestedTypes) 
                    GetInstrumentableTypes(typeDefinition.NestedTypes, classes, filter); 
            }
        }

       
        public Method[] GetMethodsForType(Class type, File[] files)
        {
            var methods = new List<Method>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetMethodsForType(typeDefinitions, type, methods, files, _filter);
            return methods.ToArray();
        }

        private static string GetFirstFile(MethodDefinition definition)
        {
            if (definition.HasBody && definition.Body.Instructions!=null)
            {
                var filePath = definition.Body.Instructions
                    .Where(x => x.SequencePoint != null && x.SequencePoint.Document != null && x.SequencePoint.StartLine != stepOverLineCode)
                    .Select(x => x.SequencePoint.Document.Url)
                    .FirstOrDefault();
                return filePath;
            }
            return null;
        }

        private static void GetMethodsForType(IEnumerable<TypeDefinition> typeDefinitions, Class type, List<Method> methods, File[] files, IFilter filter)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.FullName == type.FullName)
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.IsAbstract) continue;
                        var method = new Method
                                         {
                                             Name = methodDefinition.FullName,
                                             MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                                             IsConstructor = methodDefinition.IsConstructor,
                                             IsStatic = methodDefinition.IsStatic,
                                             IsGetter = methodDefinition.IsGetter,
                                             IsSetter = methodDefinition.IsSetter
                                         };
                        
                        if (filter.ExcludeByAttribute(methodDefinition))
                            method.SkippedDueTo = SkippedMethod.Attribute;
                        else if (filter.ExcludeByFile(GetFirstFile(methodDefinition)))
                            method.SkippedDueTo = SkippedMethod.File;

                        var definition = methodDefinition;
                        method.FileRef = files.Where(x => x.FullPath == GetFirstFile(definition))
                            .Select(x => new FileRef() {UniqueId = x.UniqueId}).FirstOrDefault();
                        methods.Add(method);
                    }
                }
                if (typeDefinition.HasNestedTypes) 
                    GetMethodsForType(typeDefinition.NestedTypes, type, methods, files, filter);
            }
        }

        public SequencePoint[] GetSequencePointsForToken(int token)
        {
            var list = new List<SequencePoint>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetSequencePointsForToken(typeDefinitions, token, list);
            return list.ToArray();
        }

        public BranchPoint[] GetBranchPointsForToken(int token)
        {
            var list = new List<BranchPoint>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetBranchPointsForToken(typeDefinitions, token, list);
            return list.ToArray();
        }

        public int GetCyclomaticComplexityForToken(int token)
        {
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            var complexity = 0;
            GetCyclomaticComplexityForToken(typeDefinitions, token, ref complexity);
            return complexity;
        }

        private static void GetSequencePointsForToken(IEnumerable<TypeDefinition> typeDefinitions, int token, List<SequencePoint> list)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in
                    typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.MetadataToken.ToInt32() == token)
                    .Where(methodDefinition => methodDefinition.Body != null && methodDefinition.Body.Instructions != null))
                {
                    UInt32 ordinal = 0;
                    foreach (var instruction in methodDefinition.Body.Instructions)
                    {
                        if (instruction.SequencePoint != null &&
                            instruction.SequencePoint.StartLine != stepOverLineCode)
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
                                            };
                            list.Add(point);
                        }
                    }
                }
                if (typeDefinition.HasNestedTypes) 
                    GetSequencePointsForToken(typeDefinition.NestedTypes, token, list);
            }
        }

        private static void GetBranchPointsForToken(IEnumerable<TypeDefinition> typeDefinitions, int token, List<BranchPoint> list)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in
                    typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.MetadataToken.ToInt32() == token)
                    .Where(methodDefinition => methodDefinition.Body != null && methodDefinition.Body.Instructions != null))
                {
                    UInt32 ordinal = 0;
                    foreach (var instruction in methodDefinition.Body.Instructions)
                    {
                        if (instruction.OpCode.FlowControl != FlowControl.Cond_Branch) continue;
                        if (instruction.OpCode.Code != Code.Switch)
                        {
                            list.Add(new BranchPoint() { Offset = instruction.Offset, Ordinal = ordinal++, Path = 0 });
                            list.Add(new BranchPoint() { Offset = instruction.Offset, Ordinal = ordinal++, Path = 1 });
                        }
                        else
                        {
                            for (var i = 0; i < (instruction.Operand as Instruction[]).Count() + 1; i++)
                            {
                                list.Add(new BranchPoint() { Offset = instruction.Offset, Ordinal = ordinal++, Path = i });
                            }
                        }
                    }
                }
                if (typeDefinition.HasNestedTypes) 
                    GetBranchPointsForToken(typeDefinition.NestedTypes, token, list);
            }
        }

        private static void GetCyclomaticComplexityForToken(IEnumerable<TypeDefinition> typeDefinitions, int token, ref int complexity)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in
                    typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.MetadataToken.ToInt32() == token)
                    .Where(methodDefinition => methodDefinition.Body != null && methodDefinition.Body.Instructions != null))
                {
                    complexity = Gendarme.Rules.Maintainability.AvoidComplexMethodsRule.GetCyclomaticComplexity(methodDefinition);
                }
                if (typeDefinition.HasNestedTypes) 
                    GetCyclomaticComplexityForToken(typeDefinition.NestedTypes, token, ref complexity);
            }
        }

        public TrackedMethod[] GetTrackedMethods()
        {
            if (SourceAssembly==null) return null;
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where customAttribute.AttributeType.FullName == "NUnit.Framework.TestAttribute"
                    select new TrackedMethod()
                               {
                                   MetadataToken = methodDefinition.MetadataToken.ToInt32(), Name = methodDefinition.FullName
                               }).ToArray();
        }
    }
}
