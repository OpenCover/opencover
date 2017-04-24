using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var assemblyPath = typeof(Microsoft.Practices.ServiceLocation.ServiceLocator).Assembly.Location;

            var symbolFile = SymbolFileHelper.FindSymbolFolder(assemblyPath, commandLine.Object);

            Assert.NotNull(symbolFile);
            Assert.IsInstanceOf<PdbReaderProvider>(symbolFile.SymbolReaderProvider);
            Assert.IsTrue(symbolFile.SymbolFilename.EndsWith(".pdb", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
