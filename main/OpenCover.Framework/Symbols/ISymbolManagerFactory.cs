namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManagerFactory
    {
        ISymbolManager CreateSymbolManager(string modulePath);
    }
}
