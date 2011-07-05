using System;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Common;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Service;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Communication
{
    [TestFixture]
    public class MessageHandlerTests :
        UnityAutoMockContainerBase<IMessageHandler, MessageHandler>
    {
        [Test]
        public void Handles_MSG_TrackAssembly()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_TrackAssembly_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_TrackAssembly_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_TrackAssembly, IntPtr.Zero, (x) => { });

            // assert
            Container.GetMock<IProfilerCommunication>()
                .Verify(x=>x.TrackAssembly(It.IsAny<string>(), It.IsAny<string>()), Times.Once());

        }

        [Test]
        public void Handles_MSG_GetSequencePoints()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetSequencePoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetSequencePoints_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, IntPtr.Zero, (x) => { });

            // assert
            SequencePoint[] points;
            Container.GetMock<IProfilerCommunication>()
                .Verify(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points), Times.Once());

        }

        [Test]
        public void Handles_MSG_GetSequencePoints_Small()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetSequencePoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetSequencePoints_Request());

            var points = new []{new SequencePoint(), new SequencePoint()};
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));

            var mockHarness = new Mock<IProfilerManager>();

            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, IntPtr.Zero, (x) => { chunked = true; });
            
            // assert
            Container.GetMock<IMarshalWrapper>()
                .Verify(x=>x.StructureToPtr(It.IsAny<MSG_SequencePoint>(), It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Exactly(2));

            Assert.False(chunked);
        }

        [Test]
        public void Handles_MSG_GetSequencePoints_Large_StartsToChunk()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetSequencePoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetSequencePoints_Request());

            var points = new[] { new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint() };
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));
            
            var mockHarness = new Mock<IProfilerManager>();

            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, IntPtr.Zero, (x) => { chunked = true; });

            // assert
            Container.GetMock<IMarshalWrapper>()
                .Verify(x => x.StructureToPtr(It.IsAny<MSG_SequencePoint>(), It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Exactly(6));

            Assert.True(chunked);

        }

        [Test]
        public void ReadSize_Returns()
        {
            var size = Instance.ReadSize;
            Assert.AreNotEqual(0, size);
        }

        [Test]
        public void MaxMsgSize_Returns()
        {
            var size = Instance.MaxMsgSize;
            Assert.AreNotEqual(0, size);
        }

        [Test]
        public void ReceiveResults_Converts_VisitPoints()
        {
            // arrange
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_SendVisitPoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_SendVisitPoints_Request() {count = 2});

            uint uniqueid = 0;
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_VisitPoint>(It.IsAny<IntPtr>()))
                .Returns<IntPtr>(p => new MSG_VisitPoint() { UniqueId = ++uniqueid, VisitType = VisitType.SequencePoint });

            VisitPoint[] list = null;
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.Visited(It.IsAny<VisitPoint[]>()))
                .Callback<VisitPoint[]>((x) => list = x);

            // act
            Instance.ReceiveResults(IntPtr.Zero);

           
            // assert
            Assert.NotNull(list);
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(1, list[0].UniqueId);
            Assert.AreEqual(2, list[1].UniqueId);
        }

    }
}
