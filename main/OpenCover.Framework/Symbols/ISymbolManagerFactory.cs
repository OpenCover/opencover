//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace OpenCover.Framework.Symbols
{
    public interface ISymbolManagerFactory
    {
        ISymbolManager CreateSymbolManager(string modulePath, string moduleName);
    }
}
