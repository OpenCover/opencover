using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e => e(dict));

            // assert
            Assert.AreEqual(1, dict.Count);
            Assert.NotNull(dict[@"OpenCover_Profiler_Key"]);
        }

        [Test]
        public void Manager_Handles_StandardMessageEvent()
        {
            // arrange
            Container.GetMock<IMessageHandler>()
                .Setup(x => x.StandardMessage(It.IsAny<MSG_Type>(), It.IsAny<IntPtr>(), It.IsAny<Action<int>>()))
                .Callback<MSG_Type, IntPtr, Action<int>>((t, p, action) => action(0));

            // act
            var dict = new StringDictionary();

            Instance.RunProcess(e =>
                                    {
                                        e(dict);

                                        var standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                                            @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"]);

                                        standardMessageDataReady.Set();

                                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, 250));
                                        
                                        standardMessageDataReady.Set();
                                        
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

            Instance.RunProcess(e =>
            {
                e(dict);

                var standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendResults_Event_" + dict[@"OpenCover_Profiler_Key"]);

                standardMessageDataReady.Set();

                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 250));
            });

            // assert
            Container.GetMock<IMessageHandler>()
                .Verify(x => x.ReceiveResults(It.IsAny<IntPtr>()), Times.Exactly(2));
        }
    }
}
