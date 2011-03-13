using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using NUnit.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolReaderWrapperTests
    {
        [Test]
        public void GetSymbolReader_ReturnsNull_WhenFileNotFound()
        {
            // arrange
            var binder = new SymBinder();

            // act
            var x = SymbolReaderWapper.GetSymbolReader(binder, "Xyz.dll", null);
            
            // assert
            Assert.IsNull(x);
        }

        [Test]
        public void GetSymbolReader_ReturnsNotNull_WhenFileFound()
        {
            // arrange
            var location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Framework.dll");
            var binder = new SymBinder();

            // act
            var x = SymbolReaderWapper.GetSymbolReader(binder, location, null);

            // assert
            Assert.IsNotNull(x);
        }
    }
}
