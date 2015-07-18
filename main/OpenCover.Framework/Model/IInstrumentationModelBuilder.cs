//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using Mono.Cecil;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// defines an InstrumentationModelBuilder
    /// </summary>
    public interface IInstrumentationModelBuilder
    {
        /// <summary>
        /// Build model for a module
        /// </summary>
        /// <param name="full">include class, methods etc</param>
        /// <returns></returns>
        Module BuildModuleModel(bool full);

        /// <summary>
        /// Build a model with tracked tests
        /// </summary>
        /// <param name="module"></param>
        /// <param name="full">include class, methods etc</param>
        /// <returns></returns>
        Module BuildModuleTestModel(Module module, bool full);
        
        /// <summary>
        /// check if module can be instrumented
        /// </summary>
        bool CanInstrument { get; }

        /// <summary>
        /// get hold of the underlying assembly definition
        /// </summary>
        AssemblyDefinition GetAssemblyDefinition { get; }
    }
}