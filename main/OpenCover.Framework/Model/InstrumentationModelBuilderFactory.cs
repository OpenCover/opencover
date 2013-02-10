//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using System.Linq;
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

        public InstrumentationModelBuilderFactory(ICommandLine commandLine, IFilter filter, ILog logger, IEnumerable<ITrackedMethodStrategy> trackedMethodStrategies)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
            MethodStrategies = trackedMethodStrategies;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter, _logger, MethodStrategies);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }

        public IEnumerable<ITrackedMethodStrategy> MethodStrategies { get; private set; }
    }
}