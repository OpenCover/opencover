//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Symbols
{
    public class CecilSymbolManagerFactory : ISymbolManagerFactory
    {
        private readonly ICommandLine _commandLine;

        public CecilSymbolManagerFactory(ICommandLine commandLine)
        {
            _commandLine = commandLine;
        }

        public ISymbolManager CreateSymbolManager(string modulePath)
        {
            var manager = new CecilSymbolManager(_commandLine);
            manager.Initialise(modulePath);
            return manager;
        }
    }
}
