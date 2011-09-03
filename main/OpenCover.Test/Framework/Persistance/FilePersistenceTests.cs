using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using File = System.IO.File;

namespace OpenCover.Test.Framework.Persistance
{
    [TestFixture]
    public class FilePersistenceTests
    {
        private FilePersistance _persistence;
        private string _filePath;
        private TextWriter _textWriter;
        private Mock<ICommandLine> _mockCommandLine;

        [SetUp]
        public void SetUp()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _persistence = new FilePersistance(_mockCommandLine.Object);
            _persistence.Initialise(_filePath);
            _textWriter = Console.Out;
            var stringWriter = new StringWriter(new StringBuilder());
            Console.SetOut(stringWriter);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
            Console.SetOut(_textWriter);
        }

        [Test]
        public void Commit_CreatesFile()
        {
            // arrange
            _persistence.PersistModule(new Module(){Classes = new Class[0]});

            // act
            _persistence.Commit();

            // assert
            Assert.IsTrue(File.Exists(_filePath));
        }

    }
}
