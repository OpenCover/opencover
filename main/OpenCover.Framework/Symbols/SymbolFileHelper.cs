using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace OpenCover.Framework.Symbols
{
    internal static class SymbolFileHelper
    {
        /// <summary>
        /// Locate symbol files (pbd/mdb) that are noto in the same folder as the assembly
        /// NOTE: due to test frameworks copying just assemblies and not the symbol files 
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        public static SymbolFile FindSymbolFolder(string modulePath, ICommandLine commandLine)
        {
            var origFolder = Path.GetDirectoryName(modulePath);

            var searchFolders = new List<string> {origFolder, commandLine.TargetDir};
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
                    {
                        if (IsPortablePdb(symbolFile))
                        {
                            return new SymbolFile(symbolFile, new PortablePdbReaderProvider());
                        }
                        return new SymbolFile(symbolFile, new PdbReaderProvider());
                    }

                    symbolFile = Path.Combine(targetfolder, fileName + ".mdb");
                    if (File.Exists(symbolFile))
                    {
                        return new SymbolFile(symbolFile, new MdbReaderProvider());
                    }
                }
            }
            return null;
        }

        private static bool IsPortablePdb(string pdbFile)
        {
            if (File.Exists(pdbFile))
            {
                try
                {
                    // HACK: is it portable see (https://github.com/jbevain/cecil/issues/282#issuecomment-234732197)
                    const uint portablePdbSignature = 0x424a5342;
                    using (var stream = File.Open(pdbFile, FileMode.Open))
                    {
                        var buffer = new byte[4];
                        if (4 == stream.Read(buffer, 0, 4))
                        {
                            return BitConverter.ToInt32(buffer, 0) == portablePdbSignature;
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return false;
        }
    }
}