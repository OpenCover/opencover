namespace OpenCover.Framework.Symbols
{
    public class SymbolManagerFactory : ISymbolManagerFactory
    {
        private readonly ICommandLine _commandLine;
        private readonly ISymbolReaderFactory _symbolReaderFactory;

        public SymbolManagerFactory(ICommandLine commandLine, ISymbolReaderFactory symbolReaderFactory)
        {
            _commandLine = commandLine;
            _symbolReaderFactory = symbolReaderFactory;
        }

        public ISymbolManager CreateSymbolManager(string modulePath)
        {
            var manager = new SymbolManager(_commandLine, _symbolReaderFactory);
            manager.Initialise(modulePath);
            return manager;
        }
    }
}