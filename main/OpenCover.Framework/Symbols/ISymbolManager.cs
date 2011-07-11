//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManager
    {
        string ModulePath { get; }

        string ModuleName { get; }

        File[] GetFiles();

        Class[] GetInstrumentableTypes();

        Method[] GetConstructorsForType(Class type, File[] files);

        Method[] GetMethodsForType(Class type, File[] files);

        SequencePoint[] GetSequencePointsForToken(int token);

        int GetCyclomaticComplexityForToken(int token);

        AssemblyDefinition SourceAssembly { get; }
    }
}