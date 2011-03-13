using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolManagerTests
    {
        private SymbolManager _reader;
        private string _location;

        [SetUp]
        public void Setup()
        {
            var factory = new SymbolReaderFactory();
            _location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll");

            _reader = new SymbolManager(_location, null, factory);
        }

        [TearDown]
        public void Teardown()
        {
            _reader.Dispose();
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
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.DeclaredConstructorClass").First();

            // act
            var ctors = _reader.GetConstructorsForType(type);

            // assert
            Assert.IsNotNull(ctors);
        }

        [Test]
        public void GetMethodsForType_Returns_AllDeclared_ForType()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.DeclaredMethodClass").First();

            // act
            var methods = _reader.GetMethodsForType(type);

            // assert
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetSequencePointsForMethodToken()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.DeclaredMethodClass").First();
            var methods = _reader.GetMethodsForType(type);

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
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.DeclaredConstructorClass").First();
            var ctors = _reader.GetConstructorsForType(type);

            // act
            var points = _reader.GetSequencePointsForToken(ctors[0].MetadataToken);

            // assert
            Assert.IsNotNull(points);

        }

        [Test]
        public void GetSequencePointsForToken_HandlesErrors()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.ConstructorNotDeclaredClass").First();
            var ctors = _reader.GetConstructorsForType(type);

            // act
            var points = _reader.GetSequencePointsForToken(ctors[0].MetadataToken);

            // assert
            Assert.IsNull(points);

        }

        [Test]
        public void ModulePath_Returns_Name_Of_Module()
        {
            Assert.AreEqual(_location, _reader.ModulePath);
        }
    }
}
