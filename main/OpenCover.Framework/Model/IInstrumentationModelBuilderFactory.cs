namespace OpenCover.Framework.Model
{
    public interface IInstrumentationModelBuilderFactory
    {
        IInstrumentationModelBuilder CreateModelBuilder(string moduleName);
    }
}