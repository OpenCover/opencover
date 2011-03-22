using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Service
{
    [TestFixture]
    public class ProfilerCommunicationTests :
        UnityAutoMockContainerBase<IProfilerCommunication, ProfilerCommunication>
    {
        [Test]
        public void TrackAssembly_Adds_AssemblyToModel_If_FilterUseAssembly_Returns_True()
        {
            // arrange
            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            Container.GetMock<IInstrumentationModelBuilderFactory>()
                .Setup(x => x.CreateModelBuilder(It.IsAny<string>()))
                .Returns(new Mock<IInstrumentationModelBuilder>().Object);

            // act
            var track = Instance.TrackAssembly("moduleName", "assemblyName");
            
            // assert
            Assert.IsTrue(track);
            Container.GetMock<IPersistance>()
                .Verify(x=>x.PersistModule(It.IsAny<Module>()), Times.Once());
        }

        [Test]
        public void TrackAssembly_DoesntAdd_AssemblyToModel_If_FilterUseAssembly_Returns_False()
        {
            // arrange
            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(false);

            // act
            var track = Instance.TrackAssembly("moduleName", "assemblyName");

            // assert
            Assert.IsFalse(track);
            Container.GetMock<IPersistance>()
                .Verify(x => x.PersistModule(It.IsAny<Module>()), Times.Never());
        }

        [Test]
        public void Stopping_Forces_Model_Finalise()
        {
            // arrange

            // act
            Instance.Stopping();

            // assert
            Container.GetMock<IPersistance>()
                .Verify(x => x.Commit(), Times.Once());
        }

        [Test]
        public void GetSequencePoints_Returns_False_When_No_Data_In_Model()
        {
            // arrange
            SequencePoint[] points;
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(false);

            // act
            InstrumentPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetSequencePoints_Returns_SequencePoints_When_Data_In_Model()
        {
            // arrange
            var points = new []{new SequencePoint()};
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(true);

            // act
            InstrumentPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual(points.GetLength(0), instrumentPoints.GetLength(0));
        }
    }
}
