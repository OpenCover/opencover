using System;
using System.Collections.Specialized;
using System.Threading;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
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
        private IMemoryManager _manager;
        private string _key;

        public override void OnSetup()
        {
            _key = (new Random().Next()).ToString();
            _manager = new MemoryManager();
            _manager.Initialise("Local", _key);
            _manager.AllocateMemoryBuffer(65536, 0);
            Container.RegisterInstance(_manager);
        }

        public override void OnTeardown()
        {
            _manager.Dispose();
        }

        [Test]
        public void Manager_Adds_Key_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_Key"]);
        }

        [Test]
        public void Manager_Adds_Default_Threshold_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_Threshold"]);
            Assert.AreEqual("0", dict[@"OpenCover_Profiler_Threshold"]);
        }

        [Test]
        public void Manager_Adds_Supplied_Threshold_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.Threshold).Returns(500);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_Threshold"]);
            Assert.AreEqual("500", dict[@"OpenCover_Profiler_Threshold"]);
        }

        [Test]
        public void Manager_Adds_TraceByTest_EnvironmentVariable_When_Tracing_Enabled()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.TraceByTest).Returns(true);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_TraceByTest"]);
            Assert.AreEqual("1", dict[@"OpenCover_Profiler_TraceByTest"]);
        }

        [Test]
        public void Manager_DoesNotAdd_TraceByTest_EnvironmentVariable_When_Tracing_Disabled()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.TraceByTest).Returns(false);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsNull(dict[@"OpenCover_Profiler_TraceByTest"]);
        }

        [Test]
        public void Manager_Adds_Cor_Profiler_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}".ToUpper(), dict[@"Cor_Profiler"].ToUpper());
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}".ToUpper(), dict[@"CoreClr_Profiler"].ToUpper());
        }

        [Test]
        public void Manager_Adds_Cor_Enable_Profiling_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.AreEqual("1", dict[@"Cor_Enable_Profiling"]);
            Assert.AreEqual("1", dict[@"CoreClr_Enable_Profiling"]);
        }

        [Test]
        public void Manager_DoesNotAdd_Cor_Profiler_Path_EnvironmentVariable_WithNormalRegistration()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.Registration).Returns(Registration.Normal);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsFalse(dict.ContainsKey(@"Cor_Profiler_Path"));
        }

        [Test]
        public void Manager_DoesNotAdd_Cor_Profiler_Path_EnvironmentVariable_WithUserRegistration()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.Registration).Returns(Registration.User);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsFalse(dict.ContainsKey(@"Cor_Profiler_Path"));
        }

        [Test]
        public void Manager_DoesNotAdd_Cor_Profiler_Path_EnvironmentVariable_WithPath32Registration()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.Registration).Returns(Registration.Path32);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsTrue(dict.ContainsKey(@"Cor_Profiler_Path"));
        }

        [Test]
        public void Manager_DoesNotAdd_Cor_Profiler_Path_EnvironmentVariable_WithPath64Registration()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.Registration).Returns(Registration.Path64);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsTrue(dict.ContainsKey(@"Cor_Profiler_Path"));
        }

        [Test, RequiresMTA]
        public void Manager_Handles_Shared_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                     .Callback(() =>
                         {
                             if (standardMessageReady != null) 
                                 standardMessageReady.Reset();
                             offloadComplete.Set();
                         });

            // act
            var dict = new StringDictionary();
            RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; }, () => 
                {
                    offloadComplete.WaitOne();
                });

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
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            using(var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, -5))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, -5))
            {
                Container.GetMock<ICommunicationManager>()
                         .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                         .Callback<IManagedCommunicationBlock, Action<IManagedCommunicationBlock, IManagedMemoryBlock>>((_, offload) =>
                         {
                             standardMessageReady.Reset();

                             offload(mcb, mmb);
                             offloadComplete.Set();
                         });

                // act
                var dict = new StringDictionary();
                RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; }, () =>
                    {
                        offloadComplete.WaitOne();
                        mmb.ProfilerHasResults.Set();
                        mmb.ProfilerHasResults.Reset();
                    });
            }
            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()), Times.Once());
        }

        [Test, RequiresMTA]
        public void Manager_Handles_Profiler_ResultsReady()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            using(var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 1))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 1))
            {
                Container.GetMock<ICommunicationManager>()
                         .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                         .Callback<IManagedCommunicationBlock, Action<IManagedCommunicationBlock, IManagedMemoryBlock>>((_, offload) =>
                         {
                             standardMessageReady.Reset();

                             offload(mcb, mmb);
                             offloadComplete.Set();
                         });

                // act
                var dict = new StringDictionary();
                RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; }, () =>
                    {
                        offloadComplete.WaitOne();
                        mcb.ProfilerRequestsInformation.Set();
                        mcb.ProfilerRequestsInformation.Reset();
                    });
            }
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
            RunSimpleProcess(dict);

            // assert
            Container.GetMock<IPersistance>().Verify(x => x.SaveVisitData(It.IsAny<byte[]>()), Times.Once());
        }

        private void RunSimpleProcess(StringDictionary dict)
        {
            RunProcess(dict, standardMessageDataReady => { }, () => { });
        }

        private void RunProcess(StringDictionary dict, Action<EventWaitHandle> getStandardMessageDataReady, Action doExtraWork)
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
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                getStandardMessageDataReady(standardMessageDataReady);

                standardMessageDataReady.Set();

                doExtraWork();

            }, false);
        }
    }
}
