using System;
using System.IO;
using Mono.Cecil.Mdb;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolFileHelperMdbTests : BaseMdbTests
    {
        [Test]
        public void CanFindAndLoadProviderForMdbFile()
        {
            var commandLine = new Mock<ICommandLine>();
            var assemblyPath = Path.GetDirectoryName(TargetType.Assembly.Location);
            var location = Path.Combine(assemblyPath, "Mdb", TargetAssembly);

            var symbolFile = SymbolFileHelper.FindSymbolFolder(location, commandLine.Object);

            Assert.NotNull(symbolFile);
            Assert.IsInstanceOf<MdbReaderProvider>(symbolFile.SymbolReaderProvider);
            Assert.IsTrue(symbolFile.SymbolFilename.EndsWith(".dll.mdb", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}