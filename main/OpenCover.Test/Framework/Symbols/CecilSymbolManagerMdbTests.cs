using System;
using System.Diagnostics;
using System.IO;
using log4net;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Symbols;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerMdbTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(Microsoft.Practices.ServiceLocation.ServiceLocator).Assembly.Location);

            var folder = Path.Combine(assemblyPath, "Mdb");
            var source = Path.Combine(assemblyPath, "Microsoft.Practices.ServiceLocation.dll");
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, "Microsoft.Practices.ServiceLocation.dll");
            File.Copy(source, dest);
            File.Copy(Path.ChangeExtension(source, "pdb"), Path.ChangeExtension(dest, "pdb"));
            var process = new ProcessStartInfo
            {
                FileName = Path.Combine(assemblyPath, @"..\..\packages\Mono.pdb2mdb.0.1.0.20130128\tools\pdb2mdb.exe"),
                Arguments = dest,
                WorkingDirectory = folder,
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            var proc = Process.Start(process);
            proc.Do(_ => _.WaitForExit());

            Assert.IsTrue(File.Exists(dest + ".mdb"));
            File.Delete(Path.ChangeExtension(dest, "pdb"));
            Assert.IsFalse(File.Exists(Path.ChangeExtension(dest, "pdb")));
        }

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

            var assemblyPath = Path.GetDirectoryName(typeof(Microsoft.Practices.ServiceLocation.ServiceLocator).Assembly.Location);
            _location = Path.Combine(assemblyPath, "Mdb", "Microsoft.Practices.ServiceLocation.dll");

            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null);
            _reader.Initialise(_location, "Microsoft.Practices.ServiceLocation");
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