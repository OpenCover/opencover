using System.IO;
using log4net;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    /// <summary>
    /// This test fails when running under Resharper and shadow copy is turned on
    /// Disable it using Resharper -> Options -> Unit Testing
    /// </summary>
    [TestFixture]
    public class CecilSymbolManagerMdbTests : BaseMdbTests
    {
        private CecilSymbolManager _reader;
        private string _location;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<IFilter> _mockFilter;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _mockFilter = new Mock<IFilter>();
            _mockLogger = new Mock<ILog>();

            var assemblyPath = Path.GetDirectoryName(TargetType.Assembly.Location);
            _location = Path.Combine(assemblyPath, "Mdb", TargetAssembly);

            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null);
            _reader.Initialise(_location, "Unity.ServiceLocation");
        }

        [Test]
        public void GetFiles_Returns_AllFiles_InModule()
        {
            //arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var files = _reader.GetFiles();

            //assert
            Assert.NotNull(files);
            Assert.AreNotEqual(0, files.GetLength(0));
        }
    }
}