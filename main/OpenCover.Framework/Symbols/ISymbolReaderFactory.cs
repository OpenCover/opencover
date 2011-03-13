using System.Diagnostics.SymbolStore;

namespace OpenCover.Framework.Symbols
{
    public interface ISymbolReaderFactory
    {
        ISymbolReader GetSymbolReader(string moduleName, string searchPath);
    }
}