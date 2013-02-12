using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Manager
{
    [TestFixture]
    public class ProfilerManagerTests :
        UnityAutoMockContainerBase<IProfilerManager, ProfilerManager>
    {
        private IMemoryManager manager;

        [SetUp]
        public void Setup()
        {
            manager = new MemoryManager();
            manager.Initialise("Local", "ABC");
            manager.AllocateMemoryBuffer(65536, 0);
            Container.RegisterInstance(manager);
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
        }

        [Test]
        public void Manager_Adds_Key_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict, () => { });

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_Key"]);
        }

        [Test]
        public void Manager_Adds_Cor_Profiler_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict, () => { });

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}".ToUpper(), dict[@"Cor_Profiler"].ToUpper());
        }

        [Test]
        public void Manager_Adds_Cor_Enable_Profiling_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict, () => { });

            // assert
            Assert.AreEqual("1", dict[@"Cor_Enable_Profiling"]);
        }

        [Test, RequiresMTA]
        public void Manager_Handles_Shared_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                     .Callback(() =>
                         {
                             if (standardMessageReady != null) 
                                 standardMessageReady.Reset();
                         });

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e =>
            {
                e(dict);

                standardMessageReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                standardMessageReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
            }, false);

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), 
                    It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()), Times.Once());
        }

        [Test, RequiresMTA]
        public void Manager_Handles_Profiler_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            
            IManagedCommunicationBlock mcb = new MemoryManager.ManagedCommunicationBlock("Local", "ABC", 100, -5);
            IManagedMemoryBlock mmb = new MemoryManager.ManagedMemoryBlock("Local", "ABC", 100, -5);

            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                     .Callback<IManagedCommunicationBlock, Action<IManagedCommunicationBlock, IManagedMemoryBlock>>((_, offload) =>
                     {
                         if (standardMessageReady != null)
                             standardMessageReady.Reset();

                         offload(mcb, mmb);
                     });

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e =>
            {
                e(dict);

                standardMessageReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                standardMessageReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));

                mmb.ProfilerHasResults.Set();
                mmb.ProfilerHasResults.Reset();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));

            }, false);

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()), Times.Once());
        }

        [Test, RequiresMTA]
        public void Manager_Handles_Profiler_ResultsReady()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;

            IManagedCommunicationBlock mcb = new MemoryManager.ManagedCommunicationBlock("Local", "ABC", 100, 1);
            IManagedMemoryBlock mmb = new MemoryManager.ManagedMemoryBlock("Local", "ABC", 100, 1);

            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                     .Callback<IManagedCommunicationBlock, Action<IManagedCommunicationBlock, IManagedMemoryBlock>>((_, offload) =>
                     {
                         if (standardMessageReady != null)
                             standardMessageReady.Reset();

                         offload(mcb, mmb);
                     });

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e =>
            {
                e(dict);

                standardMessageReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                standardMessageReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));

                mcb.ProfilerRequestsInformation.Set();
                mcb.ProfilerRequestsInformation.Reset();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));

            }, false);

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(),
                    It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()), Times.Exactly(2));
        }

        [Test]
        public void Manager_SendsResults_ForProcessing()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict, () => { });

            // assert
            Container.GetMock<IPersistance>().Verify(x => x.SaveVisitData(It.IsAny<byte[]>()), Times.Once());
        }

        private void RunProcess(StringDictionary dict, Action doExtra)
        {
            // arrange
            EventWaitHandle standardMessageDataReady = null;
            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()))
                     .Returns(() =>
                         {
                             if (standardMessageDataReady != null)
                                 standardMessageDataReady.Reset();
                             return new byte[4];
                         });

            Instance.RunProcess(e =>
            {
                e(dict);

                standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendResults_Event_ABC0");

                standardMessageDataReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));

                doExtra();

            }, false);
        }
    }
}
