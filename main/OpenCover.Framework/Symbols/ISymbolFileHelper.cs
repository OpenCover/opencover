using System.Collections.Generic;

namespace OpenCover.Framework.Symbols
{
    internal interface ISymbolFileHelper
    {
        IEnumerable<SymbolFile> GetSymbolFolders(string modulePath, ICommandLine commandLine);
    }
}