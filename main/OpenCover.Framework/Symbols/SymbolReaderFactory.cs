using System.Diagnostics.SymbolStore;

namespace OpenCover.Framework.Symbols
{
    public class SymbolReaderFactory : ISymbolReaderFactory
    {
        private readonly SymBinder _symBinder;

        public SymbolReaderFactory()
        {
            _symBinder = new SymBinder();
        }

        public ISymbolReader GetSymbolReader(string moduleName, string searchPath)
        {
            return SymbolReaderWapper.GetSymbolReader(_symBinder, moduleName, searchPath);
        }
    }
}
