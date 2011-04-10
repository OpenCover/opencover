using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using OpenCover.Framework.Model;
using File = OpenCover.Framework.Model.File;

namespace OpenCover.Framework.Symbols
{
    public class CecilSymbolManager : ISymbolManager
    {
        private readonly ICommandLine _commandLine;
        private string _modulePath;
        private AssemblyDefinition _sourceAssembly;

        public CecilSymbolManager(ICommandLine commandLine)
        {
            _commandLine = commandLine;
        }

        public string ModulePath
        {
            get { return _modulePath; }
        }

        public void Initialise(string modulePath)
        {
            _modulePath = modulePath;
        }

        private AssemblyDefinition SourceAssembly
        {
            get
            {
                if (_sourceAssembly==null)
                {
                    var resolver = new DefaultAssemblyResolver();
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(_modulePath)) == false)
                    {
                        resolver.AddSearchDirectory(Path.GetDirectoryName(_modulePath));
                        if (!string.IsNullOrEmpty(_commandLine.TargetDir))
                        {
                            resolver.AddSearchDirectory(_commandLine.TargetDir);
                        }
                    }

                    var parameters = new ReaderParameters
                    {
                        SymbolReaderProvider = new PdbReaderProvider(),
                        ReadingMode = ReadingMode.Immediate,
                        AssemblyResolver = resolver,
                    };

                    _sourceAssembly = AssemblyDefinition.ReadAssembly(_modulePath, parameters);
                    _sourceAssembly.MainModule.ReadSymbols();
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
            var classes = new List<Class>();
            foreach (var typeDefinition in SourceAssembly.MainModule.Types)
            {
                var @class = new Class() {FullName = typeDefinition.FullName};
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
                @class.Files = list.Distinct().Select(file => new File { FullPath = file }).ToArray();
                classes.Add(@class);
            }
            return classes.ToArray();
        }

        public Method[] GetConstructorsForType(Class type)
        {
            var methods = new List<Method>();
            foreach (var typeDefinition in SourceAssembly.MainModule.Types)
            {
                if (typeDefinition.FullName == type.FullName)
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.IsConstructor)
                        {
                            var method = new Method() { Name = methodDefinition.FullName, MetadataToken = methodDefinition.MetadataToken.ToInt32()};
                            methods.Add(method);
                        }
                    }
                }
            }
            return methods.ToArray();
        }

        public Method[] GetMethodsForType(Class type)
        {
            var methods = new List<Method>();
            foreach (var typeDefinition in SourceAssembly.MainModule.Types)
            {
                if (typeDefinition.FullName == type.FullName)
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (!methodDefinition.IsConstructor)
                        {
                            var method = new Method() { Name = methodDefinition.FullName, MetadataToken = methodDefinition.MetadataToken.ToInt32() };
                            methods.Add(method);
                        }
                    }
                }
            }
            return methods.ToArray();
        }

        public SequencePoint[] GetSequencePointsForToken(int token)
        {
            var list = new List<SequencePoint>();
            foreach (var typeDefinition in SourceAssembly.MainModule.Types)
            {
                foreach (var methodDefinition in typeDefinition.Methods)
                {
                    if (methodDefinition.MetadataToken.ToInt32() == token)
                    {
                        if (methodDefinition.Body != null && methodDefinition.Body.Instructions != null)
                        {
                            UInt32 ordinal = 0;
                            foreach (var instruction in methodDefinition.Body.Instructions)
                            {
                                if (instruction.SequencePoint != null)
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
                                                        VisitCount = 0
                                                    };
                                    list.Add(point);
                                }
                            }
                        }
                        return list.ToArray();
                    }
                }
            }
            return null;
        }
    }
}
