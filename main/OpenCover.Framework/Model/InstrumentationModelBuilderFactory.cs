//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Model
{
    public class InstrumentationModelBuilderFactory : IInstrumentationModelBuilderFactory
    {
        private readonly ICommandLine _commandLine;
        private readonly IFilter _filter;

        public InstrumentationModelBuilderFactory(ICommandLine commandLine, IFilter filter)
        {
            _commandLine = commandLine;
            _filter = filter;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager, _filter);
        }
    }
}