//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using log4net;

namespace OpenCover.Framework.Model
{
    public class InstrumentationModelBuilderFactory : IInstrumentationModelBuilderFactory
    {
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private readonly ITrackedMethodStrategy[] _trackedMethodStrategies;

        public InstrumentationModelBuilderFactory(ICommandLine commandLine, IFilter filter, ILog logger, ITrackedMethodStrategy[] trackedMethodStrategies)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
            _trackedMethodStrategies = trackedMethodStrategies;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter, _logger, _trackedMethodStrategies);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }
    }
}