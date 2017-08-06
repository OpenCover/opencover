//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Symbols
{
    /// <summary>
    /// defines a symbol manager for an assembly/module being profiled
    /// </summary>
    public interface ISymbolManager
    {
        /// <summary>
        /// The path to the module
        /// </summary>
        string ModulePath { get; }

        /// <summary>
        /// The name of the module
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// The source files that made up the module
        /// </summary>
        /// <returns></returns>
        File[] GetFiles();

        /// <summary>
        /// The types (with implementation) found in the module
        /// </summary>
        /// <returns></returns>
        Class[] GetInstrumentableTypes();

        /// <summary>
        /// The methods for a type in the module
        /// </summary>
        /// <param name="type"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        Method[] GetMethodsForType(Class type, File[] files);

        /// <summary>
        /// The sequence points for a method
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        SequencePoint[] GetSequencePointsForToken(int token);

        /// <summary>
        /// The branch points for a method
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        BranchPoint[] GetBranchPointsForToken(int token);

        /// <summary>
        /// The cyclomatic complexity for a method
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        int GetCyclomaticComplexityForToken(int token);

        /// <summary>
        /// The source assembly
        /// </summary>
        AssemblyDefinition SourceAssembly { get; }

        /// <summary>
        /// Test methods that are being tracked
        /// </summary>
        /// <returns></returns>
        TrackedMethod[] GetTrackedMethods();
    }
}