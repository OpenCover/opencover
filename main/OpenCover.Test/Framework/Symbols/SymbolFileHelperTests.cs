using System;
using System.IO;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolFileHelperTests 
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

            foreach (var symbolFile in _symbolFileHelper.GetSymbolFileLocations(assemblyPath, commandLine.Object))
            {
                Assert.NotNull(symbolFile);
                Assert.IsTrue(symbolFile.EndsWith(".pdb", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void CanFindAndLoadProviderForMdbFile()
        {
            var commandLine = new Mock<ICommandLine>();
            var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location) ?? Directory.GetCurrentDirectory();
            var location = Path.Combine(assemblyPath, "OpenCover.Mono.dll");

            foreach (var symbolFile in _symbolFileHelper.GetSymbolFileLocations(location, commandLine.Object))
            {
                Assert.NotNull(symbolFile);
                Assert.IsTrue(symbolFile.EndsWith(".dll.mdb", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
