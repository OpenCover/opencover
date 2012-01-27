//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace OpenCover.Framework.Model
{
    public interface IInstrumentationModelBuilder
    {
        Module BuildModuleModel(bool full);
        Module BuildModuleTestModel(Module module, bool full);
        bool CanInstrument { get; }
    }
}