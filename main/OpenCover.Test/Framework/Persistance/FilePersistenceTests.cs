using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using log4net;
using File = System.IO.File;

namespace OpenCover.Test.Framework.Persistance
{
    [TestFixture]
    public class FilePersistenceTests
    {
        private string _filePath;
        private TextWriter _textWriter;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _mockLogger = new Mock<ILog>();
            _filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            
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
            var persistence = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);
            persistence.Initialise(_filePath, false);
            persistence.PersistModule(new Module{Classes = new Class[0]});

            // act
            persistence.Commit();

            // assert
            Assert.IsTrue(File.Exists(_filePath));
        }

        [Test]
        public void CanLoadExistingFileWhenInitialising()
        {
            // arrange
            var moduleHash = Guid.NewGuid().ToString();
            var persistence = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);
            persistence.Initialise(_filePath, false);
            var point = new SequencePoint();
            var branchPoint = new BranchPoint{Path = 0, OffsetPoints = new List<int>()};
            var branchPoint2 = new BranchPoint { Path = 1, OffsetPoints = new List<int>{1,2}};
            var file = new OpenCover.Framework.Model.File();
            var filref = new FileRef() {UniqueId = file.UniqueId};

            persistence.PersistModule(new Module
            {
                Summary = new Summary {NumSequencePoints = 1},
                Files = new[] {file},
                ModuleHash = moduleHash,
                Classes = new[]
                {
                    new Class
                    {
                        Summary = new Summary {NumSequencePoints = 1},
                        Files = new[] {file},
                        Methods = new[]
                        {
                            new Method
                            {
                                FileRef = filref,
                                MetadataToken = 1234,
                                Summary = new Summary {NumSequencePoints = 1},
                                MethodPoint = point,
                                SequencePoints = new[] {point},
                                BranchPoints = new[] {branchPoint, branchPoint2}
                            }
                        }
                    }
                }
            });
            persistence.Commit();
            
            var persistence2 = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);

            // act
            persistence2.Initialise(_filePath, true);

            // assert
            Assert.IsNotNull(persistence2.CoverageSession);
            Assert.AreEqual(moduleHash, persistence2.CoverageSession.Modules[0].ModuleHash);
            Assert.AreEqual(point.UniqueSequencePoint, persistence2.CoverageSession.Modules[0].Classes[0].Methods[0].SequencePoints[0].UniqueSequencePoint);
            Assert.AreEqual(point.UniqueSequencePoint, persistence2.CoverageSession.Modules[0].Classes[0].Methods[0].MethodPoint.UniqueSequencePoint);
            var method = persistence2.CoverageSession.Modules[0].Classes[0].Methods[0];
            var br1 = persistence2.CoverageSession.Modules[0].Classes[0].Methods[0].BranchPoints[0];
            var br2 = persistence2.CoverageSession.Modules[0].Classes[0].Methods[0].BranchPoints[1];
            Assert.AreEqual(branchPoint.UniqueSequencePoint, br1.UniqueSequencePoint);
            Assert.AreEqual(branchPoint2.UniqueSequencePoint, br2.UniqueSequencePoint);
            Assert.AreEqual(0, br1.OffsetPoints.Count);
            Assert.AreEqual(2, br2.OffsetPoints.Count);
            Assert.AreEqual(1, br2.OffsetPoints[0]);
            Assert.AreEqual(2, br2.OffsetPoints[1]);

            // the method and sequence point if point to same offset need to merge
            Assert.AreSame(method.MethodPoint, method.SequencePoints[0]);

            // the loaded summary object needs to be cleared
            Assert.AreEqual(0, persistence2.CoverageSession.Summary.NumSequencePoints);
            Assert.AreEqual(0, persistence2.CoverageSession.Modules[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, persistence2.CoverageSession.Modules[0].Classes[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, persistence2.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
        }

        [Test]
        public void HandleFileAccess_SuppliedActionSuccess_ReturnsTrue()
        {
            // arrange
            var persistence = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);

            // act
            Assert.IsTrue(persistence.HandleFileAccess(() => { }, "file_name"));
        }

        [Test]
        public void HandleFileAccess_SuppliedActionThrows_Exception_ReturnsException()
        {
            // arrange
            var persistence = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);

            // act
            var expected = new Exception();
            var actual = Assert.Throws<Exception>(() => persistence.HandleFileAccess(() => { throw expected; }, "file_name"));

            // assert
            Assert.AreSame(expected, actual);
        }

        [Test]
        [TestCase(typeof(DirectoryNotFoundException))]
        [TestCase(typeof(IOException))]
        [TestCase(typeof(UnauthorizedAccessException))]
        public void HandleFileAccess_SuppliedActionThrows_Exception_ReturnsFalse(Type exception)
        {
            // arrange
            var persistence = new FilePersistance(_mockCommandLine.Object, _mockLogger.Object);

            // act
            Assert.IsFalse(persistence.HandleFileAccess(() => { throw (Exception) Activator.CreateInstance(exception); }, "file_name"));
        }
    }
}
