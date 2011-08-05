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
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Manager
{
    [TestFixture]
    public class ProfilerManagerTests :
        UnityAutoMockContainerBase<IProfilerManager, ProfilerManager>
    {
        [Test]
        public void Manager_Adds_Key_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_Key"]);
        }

        [Test]
        public void Manager_Adds_Cor_Profiler_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict);

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}".ToUpper(), dict[@"Cor_Profiler"].ToUpper());
        }

        [Test]
        public void Manager_Adds_Cor_Enable_Profiling_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunProcess(dict);

            // assert
            Assert.AreEqual("1", dict[@"Cor_Enable_Profiling"]);
        }

        [Test]
        public void Manager_Handles_StandardMessageEvent()
        {
            // arrange
            Container.GetMock<IMessageHandler>()
                .Setup(x => x.StandardMessage(It.IsAny<MSG_Type>(), It.IsAny<IntPtr>(), It.IsAny<Action<int>>()))
                .Callback<MSG_Type, IntPtr, Action<int>>((t, p, action) => { });

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e =>
            {
                e(dict);

                var standardMessageReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"]);

                standardMessageReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
            });

            // assert
            Container.GetMock<IMessageHandler>()
                .Verify(x => x.StandardMessage(It.IsAny<MSG_Type>(), It.IsAny<IntPtr>(), It.IsAny<Action<int>>()), Times.Once());

            Container.GetMock<IMessageHandler>()
                .Verify(x => x.ReceiveResults(It.IsAny<IntPtr>()), Times.Once());
        }

        [Test]
        public void Manager_Handles_ResultsEvent()
        {
            // arrange

            // act
            var dict = new StringDictionary();

            RunProcess(dict);

            // assert
            Container.GetMock<IMessageHandler>()
                .Verify(x => x.ReceiveResults(It.IsAny<IntPtr>()), Times.Exactly(2));
        }

        private void RunProcess(StringDictionary dict)
        {
            Instance.RunProcess(e =>
            {
                e(dict);

                var standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendResults_Event_" + dict[@"OpenCover_Profiler_Key"]);

                standardMessageDataReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
            });
        }
    }
}
