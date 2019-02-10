using System.Collections.Generic;

namespace OpenCover.Framework.Symbols
{
    internal interface ISymbolFileHelper
    {
        IEnumerable<string> GetSymbolFileLocations(string modulePath, ICommandLine commandLine);
    }
}