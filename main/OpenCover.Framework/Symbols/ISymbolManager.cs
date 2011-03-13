using OpenCover.Framework.Model;

namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManager
    {
        string ModulePath { get; }
        File[] GetFiles();
        Class[] GetInstrumentableTypes();
        Method[] GetConstructorsForType(Class type);
        Method[] GetMethodsForType(Class type);
        SequencePoint[] GetSequencePointsForToken(int token);
    }
}