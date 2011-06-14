using OpenCover.Framework.Model;

namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManager
    {
        string ModulePath { get; }
        File[] GetFiles();
        Class[] GetInstrumentableTypes();
        Method[] GetConstructorsForType(Class type, File[] files);
        Method[] GetMethodsForType(Class type, File[] files);
        SequencePoint[] GetSequencePointsForToken(int token);
    }
}