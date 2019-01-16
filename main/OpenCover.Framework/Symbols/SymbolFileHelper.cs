using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenCover.Framework.Symbols
{
    internal class SymbolFileHelper : ISymbolFileHelper
    {
        /// <summary>
        /// Locate symbol files (pbd/mdb) that are not in the same folder as the assembly
        /// NOTE: due to test frameworks copying just assemblies and not the symbol files 
        /// </summary>
        public IEnumerable<string> GetSymbolFileLocations(string modulePath, ICommandLine commandLine)
        {
            var origFolder = Path.GetDirectoryName(modulePath);

            var searchFolders = new List<string> {origFolder, commandLine.TargetDir};
            if (commandLine.SearchDirs != null)
                searchFolders.AddRange(commandLine.SearchDirs);
            searchFolders.Add(Environment.CurrentDirectory);

            return searchFolders.Where(searchFolder => searchFolder != null)
                .Select(searchFolder => FindSymbolFile(modulePath, searchFolder))
                .Where(symbolFolder => symbolFolder != null);
        }

        private static string FindSymbolFile(string fileName, string targetfolder)
        {
            if (!string.IsNullOrEmpty(targetfolder) && Directory.Exists(targetfolder))
            {
                var name = Path.GetFileName(fileName);
                if (name != null)
                {
                    var symbolFile = Path.Combine(targetfolder, Path.GetFileNameWithoutExtension(fileName) + ".pdb");
                    if (File.Exists(symbolFile))
                    {
                        return symbolFile;
                    }

                    symbolFile = Path.Combine(targetfolder, fileName + ".mdb");
                    if (File.Exists(symbolFile))
                    {
                        return symbolFile;
                    }
                }
            }

            return null;
        }
    }
}