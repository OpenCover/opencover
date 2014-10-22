using Mono.Cecil.Cil;

namespace OpenCover.Framework.Symbols
{
    internal class SymbolFolder
    {
        public SymbolFolder(string targetFolder, ISymbolReaderProvider symbolReaderProvider)
        {
            TargetFolder = targetFolder;
            SymbolReaderProvider = symbolReaderProvider;
        }

        public string TargetFolder { get; private set; }
        public ISymbolReaderProvider SymbolReaderProvider { get; private set; }
    }
}