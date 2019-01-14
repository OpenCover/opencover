using System;
using System.IO;
using System.Linq;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolFileHelperTests : BaseMdbTests
    {
        private readonly ISymbolFileHelper _symbolFileHelper;

        public SymbolFileHelperTests()
        {
            _symbolFileHelper = new SymbolFileHelper();
        }

        [Test]
        public void CanFindAndLoadProviderForPdbFile()
        {
            var commandLine = new Mock<ICommandLine>();
            var assemblyPath = typeof(IMocked).Assembly.Location;

            foreach (var symbolFile in _symbolFileHelper.GetSymbolFolders(assemblyPath, commandLine.Object))
            {
                Assert.NotNull(symbolFile);
                Assert.IsInstanceOf<PdbReaderProvider>(symbolFile.SymbolReaderProvider);
                Assert.IsTrue(symbolFile.SymbolFilename.EndsWith(".pdb", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void CanFindAndLoadProviderForMdbFile()
        {
            var commandLine = new Mock<ICommandLine>();
            var assemblyPath = Path.GetDirectoryName(TargetType.Assembly.Location);
            var location = Path.Combine(assemblyPath, "Mdb", TargetAssembly);

            foreach (var symbolFile in _symbolFileHelper.GetSymbolFolders(location, commandLine.Object))
            {
                Assert.NotNull(symbolFile);
                Assert.IsInstanceOf<MdbReaderProvider>(symbolFile.SymbolReaderProvider);
                Assert.IsTrue(symbolFile.SymbolFilename.EndsWith(".dll.mdb", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
