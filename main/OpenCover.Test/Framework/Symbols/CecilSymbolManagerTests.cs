using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;
using File = OpenCover.Framework.Model.File;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerTests
    {
        private CecilSymbolManager _reader;
        private string _location;
        private Mock<ICommandLine> _mockCommandLine;

        [SetUp]
        public void Setup()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll");

            _reader = new CecilSymbolManager(_mockCommandLine.Object);
            _reader.Initialise(_location, "OpenCover.Test");
        }

        [TearDown]
        public void Teardown()
        {
            //_reader.Dispose();
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
            var ctors = _reader.GetConstructorsForType(type, new File[0]);

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
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetSequencePointsForMethodToken()
        {
            // arrange
            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == "OpenCover.Test.Samples.DeclaredMethodClass").First();
            var methods = _reader.GetMethodsForType(type, new File[0]);

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
            var ctors = _reader.GetConstructorsForType(type, new File[0]);

            // act
            var points = _reader.GetSequencePointsForToken(ctors[0].MetadataToken);

            // assert
            Assert.IsNotNull(points);

        }

        [Test]
        public void GetSequencePointsForToken_HandlesUnknownTokens()
        {
            // arrange

            // act
            var points = _reader.GetSequencePointsForToken(0);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());

        }

        [Test]
        public void ModulePath_Returns_Name_Of_Module()
        {
            // arrange, act, assert
            Assert.AreEqual(_location, _reader.ModulePath);
        }

        [Test]
        public void SourceAssembly_Returns_Null_On_Failure()
        {
            // arrange
            _reader.Initialise("", "");

            // act
            var val = _reader.SourceAssembly;

            // assert
            Assert.IsNull(val);    
        }
    }
}