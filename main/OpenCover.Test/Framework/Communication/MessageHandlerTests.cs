using System;
using System.Linq;
using System.Runtime.InteropServices;
using Moq;
using NUnit.Framework;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Service;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Communication
{
    [TestFixture]
    public class MessageHandlerTests :
        UnityAutoMockContainerBase<IMessageHandler, MessageHandler>
    {
        private Mock<IManagedCommunicationBlock> _mockCommunicationBlock;
        private GCHandle _pinned;

        public override void OnSetup()
        {
            base.OnSetup();

            var data = new byte[0];
            _pinned = GCHandle.Alloc(data, GCHandleType.Pinned);

            _mockCommunicationBlock = new Mock<IManagedCommunicationBlock>();
            _mockCommunicationBlock.SetupGet(x => x.PinnedDataCommunication).Returns(_pinned);
        }

        public override void OnTeardown()
        {
            base.OnTeardown();
            _pinned.Free();
        }


        [Test]
        public void Handles_MSG_TrackAssembly()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_TrackAssembly_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_TrackAssembly_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_TrackAssembly, _mockCommunicationBlock.Object, (i, block) => { }, block => { });

            // assert
            Container.GetMock<IProfilerCommunication>()
                .Verify(x=>x.TrackAssembly(It.IsAny<string>(), It.IsAny<string>()), Times.Once());

        }

        [Test]
        public void Handles_MSG_TrackMethod()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_TrackMethod_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_TrackMethod_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_TrackMethod, _mockCommunicationBlock.Object, (i, block) => { }, block => { });

            // assert
            uint uniqueId;
            Container.GetMock<IProfilerCommunication>()
                .Verify(x => x.TrackMethod(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out uniqueId), Times.Once());

        }

        [Test]
        public void Handles_MSG_AllocateMemoryBuffer()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_AllocateBuffer_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_AllocateBuffer_Request());

            Container.GetMock<IMemoryManager>()
                     .Setup(x => x.AllocateMemoryBuffer(It.IsAny<int>(), It.IsAny<uint>()))
                     .Returns(new ManagedBufferBlock());

            // act
            Instance.StandardMessage(MSG_Type.MSG_AllocateMemoryBuffer, _mockCommunicationBlock.Object, (i, block) => { }, block => { });

            // assert
            uint uniqueId;
            Container.GetMock<IMemoryManager>()
                .Verify(x => x.AllocateMemoryBuffer(It.IsAny<int>(), It.IsAny<uint>()), Times.Once());

        }

        [Test]
        public void Handles_MSG_GetSequencePoints()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetSequencePoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetSequencePoints_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, _mockCommunicationBlock.Object, (i, block) => { }, block => { });

            // assert
            InstrumentationPoint[] points;
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

            var points = Enumerable.Repeat(new InstrumentationPoint(), 2).ToArray();
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));

            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, _mockCommunicationBlock.Object, (i, block) => { chunked = true; }, block => { });
            
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

            var points = Enumerable.Repeat(new InstrumentationPoint(), 100).ToArray();

            //var points = new[] { new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint(), new SequencePoint() };
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));
            
            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, _mockCommunicationBlock.Object, (i, block) => { chunked = true; }, block => { });

            // assert
            Container.GetMock<IMarshalWrapper>()
                .Verify(x => x.StructureToPtr(It.IsAny<MSG_SequencePoint>(), It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Exactly(100));

            Assert.True(chunked);

        }

        [Test]
        public void Handles_MSG_GetBranchPoints()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetBranchPoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetBranchPoints_Request());

            // act
            Instance.StandardMessage(MSG_Type.MSG_GetBranchPoints, _mockCommunicationBlock.Object, (i, block) => { }, block => { });

            // assert
            BranchPoint[] points;
            Container.GetMock<IProfilerCommunication>()
                .Verify(x => x.GetBranchPoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points), Times.Once());

        }

        [Test]
        public void Handles_MSG_GetBranchPoints_Small()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetBranchPoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetBranchPoints_Request());

            var points = Enumerable.Repeat(new BranchPoint(), 2).ToArray();
            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetBranchPoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));

            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetBranchPoints, _mockCommunicationBlock.Object, (i, block) => { chunked = true; }, block => { });

            // assert
            Container.GetMock<IMarshalWrapper>()
                .Verify(x => x.StructureToPtr(It.IsAny<MSG_BranchPoint>(), It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Exactly(2));

            Assert.False(chunked);
        }

        [Test]
        public void Handles_MSG_GetBranchPoints_Large_StartsToChunk()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetBranchPoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetBranchPoints_Request());

            var points = Enumerable.Repeat(new BranchPoint(), 100).ToArray();

            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetBranchPoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points));

            var chunked = false;
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetBranchPoints, _mockCommunicationBlock.Object, (i, block) => { chunked = true; }, block => { });

            // assert
            Container.GetMock<IMarshalWrapper>()
                .Verify(x => x.StructureToPtr(It.IsAny<MSG_BranchPoint>(), It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Exactly(100));

            Assert.True(chunked);
        }

        [Test]
        public void ReadSize_Returns()
        {
            var size = Instance.ReadSize;
            Assert.AreNotEqual(0, size);
        }

        [Test]
        public void WhenComplete_Stop_ProfilerCommunication()
        {
            // act
            Instance.Complete();

            // assert
            Container.GetMock<IProfilerCommunication>().Verify(x => x.Stopping(), Times.Once());
        }

        [Test]
        public void ExceptionDuring_MSG_GetSequencePoints_ReturnsLastBlockAsEmpty()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetSequencePoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetSequencePoints_Request());

            var points = Enumerable.Repeat(new SequencePoint(), 100).ToArray<InstrumentationPoint>();

            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetSequencePoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points))
                .Throws<NullReferenceException>();

            var response = new MSG_GetSequencePoints_Response(){count = -1, more = true};
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.StructureToPtr(It.IsAny<MSG_GetSequencePoints_Response>(), It.IsAny<IntPtr>(), It.IsAny<bool>()))
                .Callback<MSG_GetSequencePoints_Response, IntPtr, bool>((pts, ptr, b) => { response = pts; });

            var chunked = false;
            
            // act
            Instance.StandardMessage(MSG_Type.MSG_GetSequencePoints, _mockCommunicationBlock.Object, 
                (i, block) => { chunked = true; }, 
                block => { });

            // assert
            Assert.False(chunked);
            Assert.AreEqual(0, response.count);
            Assert.AreEqual(false, response.more);
        }

        [Test]
        public void ExceptionDuring_MSG_GetBranchPoints_ReturnsLastBlockAsEmpty()
        {
            // arrange 
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.PtrToStructure<MSG_GetBranchPoints_Request>(It.IsAny<IntPtr>()))
                .Returns(new MSG_GetBranchPoints_Request());

            var points = Enumerable.Repeat(new BranchPoint(), 100).ToArray();

            Container.GetMock<IProfilerCommunication>()
                .Setup(x => x.GetBranchPoints(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), out points))
                .Throws<NullReferenceException>();

            var response = new MSG_GetBranchPoints_Response() { count = -1, more = true };
            Container.GetMock<IMarshalWrapper>()
                .Setup(x => x.StructureToPtr(It.IsAny<MSG_GetBranchPoints_Response>(), It.IsAny<IntPtr>(), It.IsAny<bool>()))
                .Callback<MSG_GetBranchPoints_Response, IntPtr, bool>((pts, ptr, b) => { response = pts; });

            var chunked = false;

            // act
            Instance.StandardMessage(MSG_Type.MSG_GetBranchPoints, _mockCommunicationBlock.Object,
                (i, block) => { chunked = true; },
                block => { });

            // assert
            Assert.False(chunked);
            Assert.AreEqual(0, response.count);
            Assert.AreEqual(false, response.more);
        }
    }
}
