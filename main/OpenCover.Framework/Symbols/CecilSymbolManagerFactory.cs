using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Symbols
{
    public class CecilSymbolManagerFactory : ISymbolManagerFactory
    {
        public ISymbolManager CreateSymbolManager(string modulePath)
        {
            return new CecilSymbolManager(modulePath);
        }
    }
}
