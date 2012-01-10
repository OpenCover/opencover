//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace OpenCover.Framework.Model
{
    public interface IInstrumentationModelBuilder
    {
        Module BuildModuleModel();
        Module BuildModuleTestModel(Module module);
        bool CanInstrument { get; }
    }
}