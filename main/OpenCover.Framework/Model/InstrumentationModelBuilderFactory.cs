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

        public InstrumentationModelBuilderFactory(ICommandLine commandLine)
        {
            _commandLine = commandLine;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }
    }
}