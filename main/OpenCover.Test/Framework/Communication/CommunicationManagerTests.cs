using System;
using System.Collections.Generic;
using System.IO;
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

        [Test, Repeat(100)]
        public void HandleMemoryBlock_Returns_Block_Informs_Profiler_When_Read()
        {
            // arrange
            using (var wait = new AutoResetEvent(false)) {
                using (var mcb = new MemoryManager.ManagedMemoryBlock("Local", "XYZ", 100, 0, Enumerable.Empty<string>()))
                {
                    // act
                    byte[] data = null;
                    mcb.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                    mcb.StreamAccessorResults.Write(BitConverter.GetBytes(24), 0, 4); // count + 24 entries == 100 bytes
                    ThreadPool.QueueUserWorkItem(state =>
                        {
                            data = Instance.HandleMemoryBlock(mcb);
                            wait.Set();
                        });
                    wait.WaitOne();
                    
                    // assert
                    Assert.IsTrue(mcb.ResultsHaveBeenReceived.WaitOne(new TimeSpan(0, 0, 0, 4)), "Profiler wasn't signalled");
                    Assert.AreEqual(100, data.Count());
                }
            }
        }

        [Test, Repeat(100)]
        public void HandleCommunicationBlock_Informs_Profiler_When_Data_Is_Ready()
        {
            // arrange
            using (var wait = new AutoResetEvent(false))
            {
                using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", "XYZ", 100, 0,
                        Enumerable.Empty<string>()))
                {
                    // act
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        Instance.HandleCommunicationBlock(mcb, block => { });
                        wait.Set();
                    });

                    // assert
                    Assert.IsTrue(mcb.InformationReadyForProfiler.WaitOne(new TimeSpan(0, 0, 0, 4)),
                        "Profiler wasn't signalled");
                    mcb.InformationReadByProfiler.Set();
                    wait.WaitOne();

                    Container.GetMock<IMessageHandler>().Verify(x => x.StandardMessage(It.IsAny<MSG_Type>(), mcb,
                        It.IsAny<Action<int, IManagedCommunicationBlock>>(),
                        It.IsAny<Action<ManagedBufferBlock>>()), Times.Once());
                }
            }
        }
    }
}
