//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// Define the model builder factory
    /// </summary>
    public interface IInstrumentationModelBuilderFactory
    {
        /// <summary>
        /// Create a model builder for a module
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName);
    }
}