using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace OpenCover.Framework.Symbols
{
    internal class SymbolFile
    {
        public SymbolFile(string symbolFile, ISymbolReaderProvider symbolReaderProvider)
        {
            SymbolFilename = symbolFile;
            SymbolReaderProvider = symbolReaderProvider;
        }

        public string SymbolFilename { get; private set; }
        public ISymbolReaderProvider SymbolReaderProvider { get; private set; }

        public static SymbolFile FindSymbolFolder(string modulePath, ICommandLine commandLine)
        {
            var origFolder = Path.GetDirectoryName(modulePath);

            var searchFolders = new List<string> { origFolder, commandLine.TargetDir };
            if (commandLine.SearchDirs != null)
                searchFolders.AddRange(commandLine.SearchDirs);
            searchFolders.Add(Environment.CurrentDirectory);

            return searchFolders.Select(searchFolder => FindSymbolsFolder(modulePath, searchFolder))
                .FirstOrDefault(symbolFolder => symbolFolder != null);
        }

        private static SymbolFile FindSymbolsFolder(string fileName, string targetfolder)
        {
            if (!string.IsNullOrEmpty(targetfolder) && Directory.Exists(targetfolder))
            {
                var name = Path.GetFileName(fileName);
                if (name != null)
                {
                    var symbolFile = Path.Combine(targetfolder, Path.GetFileNameWithoutExtension(fileName) + ".pdb");
                    if (File.Exists(symbolFile))
                        return new SymbolFile(symbolFile, new PdbReaderProvider());

                    symbolFile = Path.Combine(targetfolder, fileName + ".mdb");
                    if (File.Exists(symbolFile))
                        return new SymbolFile(symbolFile, new MdbReaderProvider());
                }
            }
            return null;
        }
    }
}