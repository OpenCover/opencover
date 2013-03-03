using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Communication
{
    [TestFixture]
    public class CommunicationManagerTests :
        UnityAutoMockContainerBase<ICommunicationManager, CommunicationManager>
    {
        [Test]
        public void When_Complete_Casacde_Call()
        {
            // arrange

            // act
            Instance.Complete();

            // assert
            Container.GetMock<IMessageHandler>().Verify(x => x.Complete(), Times.Once());
        }

        [Test]
        public void HandleMemoryBlock_Returns_Block_Informs_Profiler_When_Read()
        {
            // arrange
            var mcb = new MemoryManager.ManagedMemoryBlock("Local", "XYZ", 100, 0);

            // act
            byte[] data = null; 
            ThreadPool.QueueUserWorkItem(state =>
                {
                    data = Instance.HandleMemoryBlock(mcb);
                });

            // assert
            Assert.IsTrue(mcb.ResultsHaveBeenReceived.WaitOne(new TimeSpan(0, 0, 0, 1)), "Profiler wasn't signalled");
            Assert.AreEqual(100, data.Count());
        }

        [Test]
        public void HandleCommunicationBlock_Informs_Profiler_When_Data_Is_Ready()
        {
            // arrange
            var mcb = new MemoryManager.ManagedCommunicationBlock("Local", "XYZ", 100, 0);

            // act
            ThreadPool.QueueUserWorkItem(state => Instance.HandleCommunicationBlock(mcb, (block, memoryBlock) => { }));

            // assert
            Assert.IsTrue(mcb.InformationReadyForProfiler.WaitOne(new TimeSpan(0, 0, 0, 1)), "Profiler wasn't signalled");
            mcb.InformationReadByProfiler.Set();

            Container.GetMock<IMessageHandler>().Verify(x => x.StandardMessage(It.IsAny<MSG_Type>(), mcb, 
                It.IsAny<Action<int, IManagedCommunicationBlock>>(), 
                It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()), Times.Once());
        }

    }
}
