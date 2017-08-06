using Mono.Cecil.Cil;

namespace OpenCover.Framework.Symbols
{
    internal class SymbolFile
    {
        public SymbolFile(string symbolFile, ISymbolReaderProvider symbolReaderProvider)
        {
            SymbolFilename = symbolFile;
            SymbolReaderProvider = symbolReaderProvider;
        }

        public string SymbolFilename { get; }
        public ISymbolReaderProvider SymbolReaderProvider { get; }
    }
}