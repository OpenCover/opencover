namespace OpenCover.Framework.Symbols
{
    internal interface ISymbolFileHelper
    {
        SymbolFile FindSymbolFolder(string modulePath, ICommandLine commandLine);
    }
}