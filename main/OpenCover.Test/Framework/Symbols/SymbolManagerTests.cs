using System;
using System.IO;
using NUnit.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolManagerTests
    {
        private SymbolManager _reader;

        [SetUp]
        public void Setup()
        {
            var factory = new SymbolReaderFactory();
            var location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Framework.dll");

            _reader = new SymbolManager(location , null, factory);
        }

        [Test]
        public void GetFiles_Returns_AllFiles_InModule()
        {
            //arrange

            // act
            var files = _reader.GetFiles();

            //assert
            Assert.NotNull(files);
            Assert.AreNotEqual(0, files.GetLength(0));
        }

        [Test]
        public void GetInstrumentableTypes_Returns_AllTypes_InModule_ThatCanBeInstrumented()
        {
            // arrange

            // act
            var types = _reader.GetInstrumentableTypes();
            
            // assert
            Assert.NotNull(types);
            Assert.AreNotEqual(0, types.GetLength(0));
        }

        [Test]
        public void GetConstructorsForType_Returns_AllDeclared_ForType()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();

            // act
            var ctors = _reader.GetConstructorsForType(types[1]);

            // assert
            Assert.IsNotNull(ctors);
        }

        [Test]
        public void GetMethodsForType_Returns_AllDeclared_ForType()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();

            // act
            var methods = _reader.GetMethodsForType(types[1]);

            // assert
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetSequencePointsForMethodToken()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var methods = _reader.GetMethodsForType(types[1]);

            // act
            var points = _reader.GetSequencePointsForToken(methods[0].MetadataToken);
            // assert

            Assert.IsNotNull(points);
        }

        [Test]
        public void GetSequencePointsForConstructorToken()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var ctors = _reader.GetConstructorsForType(types[0]);

            // act
            var points = _reader.GetSequencePointsForToken(ctors[0].MetadataToken);

            // assert
            Assert.IsNotNull(points);

        }
    }
}
