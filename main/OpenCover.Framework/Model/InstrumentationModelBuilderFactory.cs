using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using log4net;

namespace OpenCover.Framework.Model
{
    internal class InstrumentationModelBuilderFactory : IInstrumentationModelBuilderFactory
    {
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private readonly ITrackedMethodStrategyManager _trackedMethodStrategyManager;
        private readonly ISymbolFileHelper _symbolFileHelper;

        public InstrumentationModelBuilderFactory(ICommandLine commandLine, IFilter filter, ILog logger, 
            ITrackedMethodStrategyManager trackedMethodStrategyManager, ISymbolFileHelper symbolFileHelper)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
            _trackedMethodStrategyManager = trackedMethodStrategyManager;
            _symbolFileHelper = symbolFileHelper;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter, _logger, _trackedMethodStrategyManager, _symbolFileHelper);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }

    }
}