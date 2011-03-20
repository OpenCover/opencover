using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using NUnit.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolReaderFactoryTests
    {
        [Test]
        public void GetSymbolReader_ReturnsNull_WhenFileNotFound()
        {
            // arrange
            var factory = new SymbolReaderFactory();

            // act
            var x = factory.GetSymbolReader("Xyz.dll", null);
            
            // assert
            Assert.IsNull(x);
        }

        [Test]
        public void GetSymbolReader_ReturnsNotNull_WhenFileFound()
        {
            // arrange
            var factory = new SymbolReaderFactory();
            var location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Framework.dll");
 
            // act
            var x = factory.GetSymbolReader(location, null);

            // assert
            Assert.IsNotNull(x);
        }
    }
}
