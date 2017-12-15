using System;
using Mono.Cecil.Pdb;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolFileHelperTests
    {
        [Test]
        public void CanFindAndLoadProviderForPdbFile()
        {
            var commandLine = new Mock<ICommandLine>();
            var assemblyPath = typeof(IMocked).Assembly.Location;

            var symbolFile = SymbolFileHelper.FindSymbolFolder(assemblyPath, commandLine.Object);

            Assert.NotNull(symbolFile);
            Assert.IsInstanceOf<PdbReaderProvider>(symbolFile.SymbolReaderProvider);
            Assert.IsTrue(symbolFile.SymbolFilename.EndsWith(".pdb", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
