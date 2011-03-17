namespace OpenCover.Framework.Symbols
{
    public class SymbolManagerFactory : ISymbolManagerFactory
    {
        public ISymbolManager CreateSymbolManager(string modulePath, string searchPath, ISymbolReaderFactory symbolReaderFactory)
        {
            return new SymbolManager(modulePath, searchPath, symbolReaderFactory);
        }
    }
}