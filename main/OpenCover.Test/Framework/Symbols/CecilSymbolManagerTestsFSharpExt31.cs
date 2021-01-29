using System.IO;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using log4net;
using System.Linq;
using File = OpenCover.Framework.Model.File;
using System;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerTestsFSharpExt31
    {
        private CecilSymbolManager _reader;
        private string _location;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<IFilter> _mockFilter;
        private Mock<ILog> _mockLogger;
        private Mock<ITrackedMethodStrategyManager> _mockManager;
        private Mock<ISymbolFileHelper> _mockSymbolFileHelper;

        [SetUp]
        public void Setup()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _mockFilter = new Mock<IFilter>();
            _mockLogger = new Mock<ILog>();
            _mockManager = new Mock<ITrackedMethodStrategyManager>();
            _mockSymbolFileHelper = new Mock<ISymbolFileHelper>();

            var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            _location = Path.Combine(assemblyPath, "netcoreapp3.1", "OpenCover.Test.Samples.Fs.3.1.dll");

            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null, _mockSymbolFileHelper.Object);
            _reader.Initialise(_location, "OpenCover.Test.Samples.Fs.3.1");
        }

        [Test]
        public void Issue807_IgnoresBranchesGeneratedDueToInliningFSharp()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName.EndsWith("Issue807"));
            var methods = _reader.GetMethodsForType(type, new File[0]);

            var branchPoints = _reader.GetBranchPointsForToken(methods.First(x => x.FullName.Contains("::example")).MetadataToken);
            var sequencePoints = _reader.GetSequencePointsForToken(methods.First(x => x.FullName.Contains("::example")).MetadataToken);

            Assert.AreEqual(0, branchPoints.Count());

            Assert.AreEqual(2, sequencePoints.Count());
        }
    }
}