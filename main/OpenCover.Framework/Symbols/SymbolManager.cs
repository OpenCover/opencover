using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManager
    {
        File[] GetFiles();
        Class[] GetInstrumentableTypes();
        SequencePoint[] GetSequencePointsForToken(int token);
    }
    
    /// <summary>
    /// Interacts with the symbol PDB files and uses Model based entities
    /// </summary>
    public class SymbolManager
    {
        private readonly string _modulePath;
        private readonly ISymbolReader _symbolReader;
        private readonly Assembly _assembly;
 
        public SymbolManager(string modulePath, string searchPath, ISymbolReaderFactory symbolReaderFactory)
        {
            _modulePath = modulePath;
            _symbolReader = symbolReaderFactory.GetSymbolReader(modulePath, searchPath);
            _assembly = Assembly.ReflectionOnlyLoadFrom(_modulePath);
        }

        /// <summary>
        /// Get a list of files that the module was built with
        /// </summary>
        /// <returns></returns>
        public File[] GetFiles()
        {
            var docs = _symbolReader.GetDocuments();
            return docs
                .Select(doc => doc.URL)
                .Select(x=>new File(){FullPath = x})
                .ToArray();
        }

        /// <summary>
        /// Get a list of types that it should be able to instrument i.e. can have methods
        /// </summary>
        /// <returns></returns>
        public Class[] GetInstrumentableTypes()
        {
            // for now just classes but structs can have methods too
            var types = _assembly
                .GetTypes()
                .Where(EvaluateType)
                .Select(x => new Class(){FullName = x.FullName})
                .ToArray();

            return types;
        }

        private static bool EvaluateType(Type type)
        {
            if (!type.IsClass) return false;
            var att = type.GetCustomAttributesData();
            if (att.Count == 0) return true;
            if (att[0].Constructor.DeclaringType == typeof(CompilerGeneratedAttribute)) return false;
            return true;
        }

        /// <summary>
        /// Get a list of constructors for the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// The only constructors needed those that are declared in that class scope i.e. not base class methods
        /// </remarks>
        public Method[] GetConstructorsForType(Class type)
        {
            return _assembly.GetType(type.FullName).GetConstructors(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly |
                BindingFlags.NonPublic)
                .Select(x => new Method{Name = x.Name, MetadataToken = x.MetadataToken})
                .ToArray();
        }

        /// <summary>
        /// Get a list of methods fro the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// The only methods needed those that are declared in that class scope i.e. not base class methods
        /// </remarks>
        public Method[] GetMethodsForType(Class type)
        {
            return _assembly.GetType(type.FullName).GetMethods(
                BindingFlags.Instance | 
                BindingFlags.Public | 
                BindingFlags.Static |
                BindingFlags.DeclaredOnly |
                BindingFlags.NonPublic).Where(EvaluateMethodInfo)
                .Select(x => new Method { Name = x.Name, MetadataToken = x.MetadataToken })
                .ToArray();
        }

        private static bool EvaluateMethodInfo(MethodInfo methodInfo)
        {
            var att = methodInfo.GetCustomAttributesData();
            if (att.Count == 0) return true;
            if (att[0].Constructor.DeclaringType == typeof(CompilerGeneratedAttribute)) return false;
            return true;
        }

        /// <summary>
        /// Get a list of sequence points for a supplied token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public SequencePoint[] GetSequencePointsForToken(int token)
        {
            var symbolToken = new SymbolToken(token);
            var method = _symbolReader.GetMethod(symbolToken);
            var count = method.SequencePointCount;

            var offsets = new int[count];
            var sls = new int[count];
            var scs = new int[count];
            var els = new int[count];
            var ecs = new int[count];
            var docs = new ISymbolDocument[count];

            method.GetSequencePoints(offsets, docs, sls, scs, els, ecs);

            var list = new List<SequencePoint>();
            for (var i = 0; i < count; i++)
            {
                list.Add(new SequencePoint(){Ordinal = i, 
                    Offset = offsets[i], 
                    StartLine = sls[i], 
                    StartColumn = scs[i], 
                    EndLine = els[i], 
                    EndColumn = ecs[i]});
            }
            return list.ToArray();
        }
    }
}
