//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using OpenCover.Framework.Symbols;
using log4net;

namespace OpenCover.Framework.Model
{
    public class InstrumentationModelBuilderFactory : IInstrumentationModelBuilderFactory
    {
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;

        public InstrumentationModelBuilderFactory(ICommandLine commandLine, IFilter filter, ILog logger)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter, _logger);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }
    }
}