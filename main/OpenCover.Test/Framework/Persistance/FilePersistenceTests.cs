using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Common;
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
        public void IsTracking_True_IfModuleKnown()
        {
            // arrange
            var module = new Module() {FullName = "ModulePath"};
            module.Aliases.Add("ModulePath");
            _persistence.PersistModule(module);

            // act
            var tracking = _persistence.IsTracking("ModulePath");

            // assert
            Assert.IsTrue(tracking);
        }

        [Test]
        public void GetSequencePoints_GetsPoints_When_ModuleAndFunctionKnown()
        {
            // arrange
            var module = new Module() { FullName = "ModulePath", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } };

            module.Aliases.Add("ModulePath");
            _persistence.PersistModule(module);

            // act
            SequencePoint[] points;
            _persistence.GetSequencePointsForFunction("ModulePath", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(1, points.Count());
            Assert.AreEqual(1000, points[0].VisitCount);
        }

        [Test]
        public void GetSequencePoints_GetsZeroPoints_When_ModuleNotKnown()
        {
            // arrange
            _persistence.PersistModule(new Module() { FullName = "ModuleName", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } });

            // act
            SequencePoint[] points;
            _persistence.GetSequencePointsForFunction("ModuleName1", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());
        }


        [Test]
        public void GetSequencePoints_GetsZeroPoints_When_FunctionNotKnown()
        {
            // arrange
            _persistence.PersistModule(new Module() { FullName = "ModuleName", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } });

            // act
            SequencePoint[] points;
            _persistence.GetSequencePointsForFunction("ModuleName", 2, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());
        }

        [Test]
        public void SaveVisitPoints_AggregatesResults()
        {
            // arrange
            _persistence.PersistModule(new Module() { FullName = "ModuleName", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { UniqueSequencePoint = 100 } } } } } } });

            // act
            _persistence.SaveVisitPoints(new[] { new VisitPoint() { UniqueId = 100, VisitType = VisitType.SequencePoint }, new VisitPoint() { UniqueId = 100, VisitType = VisitType.SequencePoint } });

            // assert
            Assert.AreEqual(2, _persistence.CoverageSession.Modules[0].Classes[0].Methods[0].SequencePoints[0].VisitCount);
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
