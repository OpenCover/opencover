using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
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
            _manager.Initialise("Local", _key, Enumerable.Empty<string>());
            uint bufferId;
            _manager.AllocateMemoryBuffer(65536, out bufferId);
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
        public void Manager_DoesNotAdd_ShortWait_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.Null(dict[@"OpenCover_Profiler_ShortWait"]);
        }

        [Test]
        public void Manager_Adds_Supplied_ShortWait_EnvironmentVariable()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.CommunicationTimeout).Returns(10000);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_ShortWait"]);
            Assert.AreEqual("10000", dict[@"OpenCover_Profiler_ShortWait"]);
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
            Assert.IsFalse(!(dict.ContainsKey(@"Cor_Profiler_Path") || !string.IsNullOrEmpty(dict[@"Cor_Profiler_Path"])));
            Assert.IsFalse(!(dict.ContainsKey(@"CorClr_Profiler_Path") || !string.IsNullOrEmpty(dict[@"CorClr_Profiler_Path"])));
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
            Assert.IsFalse(!(dict.ContainsKey(@"Cor_Profiler_Path") || !string.IsNullOrEmpty(dict[@"Cor_Profiler_Path"])));
            Assert.IsFalse(!(dict.ContainsKey(@"CorClr_Profiler_Path") || !string.IsNullOrEmpty(dict[@"CorClr_Profiler_Path"])));
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
            Assert.IsTrue(dict.ContainsKey(@"CorClr_Profiler_Path"));
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
            Assert.IsTrue(dict.ContainsKey(@"CorClr_Profiler_Path"));
        }

        [Test, Apartment(ApartmentState.MTA)]
        public void Manager_Handles_Shared_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            using (EventWaitHandle offloadComplete = new AutoResetEvent(false))
            {
                Container.GetMock<ICommunicationManager>()
                    .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(),
                                It.IsAny<Action<ManagedBufferBlock>>()))
                    .Callback(() =>
                    {
                        if (standardMessageReady != null)
                            standardMessageReady.Reset();
                        offloadComplete.Set();
                    });

                // act
                var dict = new StringDictionary();
                RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; }, 
                    () => { offloadComplete.WaitOne(); });
            }
            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(),
                    It.IsAny<Action<ManagedBufferBlock>>()), Times.Once());
        }

        [Test, Apartment(ApartmentState.MTA), Repeat(10)]
        public void Manager_Handles_Profiler_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            using (EventWaitHandle offloadComplete = new AutoResetEvent(false))
            {
                var blockHandled = new ManualResetEvent(false);

                Container.GetMock<ICommunicationManager>()
                    .Setup(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()))
                    .Returns<IManagedMemoryBlock>(mmb =>
                    {
                        mmb.ProfilerHasResults.Reset();
                        blockHandled.Set();
                        return new byte[4];
                    });

                using (
                    var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, -5,
                        Enumerable.Empty<string>()))
                using (
                    var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, -5, Enumerable.Empty<string>()))
                {
                    Container.GetMock<ICommunicationManager>()
                        .Setup(
                            x =>
                                x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(),
                                    It.IsAny<Action<ManagedBufferBlock>>()))
                        .Callback<IManagedCommunicationBlock, Action<ManagedBufferBlock>>((_, offload) =>
                        {
                            standardMessageReady.Reset();

                            offload(new ManagedBufferBlock {CommunicationBlock = mcb, MemoryBlock = mmb});
                            offloadComplete.Set();
                        });

                    // act
                    var dict = new StringDictionary();
                    RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; },
                        () =>
                        {
                            offloadComplete.WaitOne();
                            mmb.ProfilerHasResults.Set();
                            blockHandled.WaitOne();
                        });
                }
            }

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()), Times.Once());
        }

        [Test, Apartment(ApartmentState.MTA), Repeat(10)]
        public void Manager_Handles_Profiler_ResultsReady()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 2, Enumerable.Empty<string>()))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 2, Enumerable.Empty<string>()))
            {
                Container.GetMock<ICommunicationManager>()
                         .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<ManagedBufferBlock>>()))
                         .Callback<IManagedCommunicationBlock, Action<ManagedBufferBlock>>((_, offload) =>
                         {
                             standardMessageReady.Reset();
                             mcb.ProfilerRequestsInformation.Reset();

                             offload(new ManagedBufferBlock{CommunicationBlock = mcb, MemoryBlock = mmb});
                             offloadComplete.Set();
                         });

                // act
                var dict = new StringDictionary();
                RunProcess(dict, standardMessageDataReady => { standardMessageReady = standardMessageDataReady; }, () =>
                    {
                        offloadComplete.WaitOne();
                        mcb.ProfilerRequestsInformation.Set();
                        offloadComplete.WaitOne();
                    });
            }

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(),
                    It.IsAny<Action<ManagedBufferBlock>>()), Times.Exactly(2));
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

        [Test]
        public void Manager_Sets_Service_ACLs_On_Events()
        {
            // arrange
            var networkServiceSid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            var networkServiceAccount = networkServiceSid.Translate(typeof(NTAccount));
            var servicePrincipal = new[] { networkServiceAccount.ToString() };
            var self = WindowsIdentity.GetCurrent().User;

            // act
            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 2, servicePrincipal))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 2, servicePrincipal))
            {
                var phrRules = mmb.ProfilerHasResults.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier));
                var rhbrRules = mmb.ResultsHaveBeenReceived.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier));
                var priRules = mcb.ProfilerRequestsInformation.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier));
                var irfpRules = mcb.InformationReadyForProfiler.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier));
                var irbpRules = mcb.InformationReadByProfiler.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier));

                var rules = new[] { phrRules, rhbrRules, priRules, irfpRules, irbpRules };

                // assert
                foreach (var ruleset in rules)
                {
                    Assert.That(ruleset.Count, Is.EqualTo(2));

                    Assert.That(ruleset.Cast<AccessRule>().Any(r => r.IdentityReference == networkServiceSid));
                    Assert.That(ruleset.Cast<AccessRule>()
                        .Where(r => r.IdentityReference == networkServiceSid)
                        .Any(r => r.AccessControlType == AccessControlType.Allow));

                    Assert.That(ruleset.Cast<AccessRule>().Any(r => r.IdentityReference == self));
                    Assert.That(ruleset.Cast<AccessRule>()
                        .Where(r => r.IdentityReference == self)
                        .Any(r => r.AccessControlType == AccessControlType.Allow));
                }
            }
        }

        [Test]
        public void Manager_Sets_Service_ACLs_On_Memory()
        {
            // arrange
            var networkServiceSid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            var networkServiceAccount = networkServiceSid.Translate(typeof(NTAccount));
            var servicePrincipal = new[] { networkServiceAccount.ToString() };
            var self = WindowsIdentity.GetCurrent().User;

            // act
            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 2, servicePrincipal))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 2, servicePrincipal))
            {
                var mcbRules = mcb.MemoryAcl.GetAccessRules(true, false, typeof(SecurityIdentifier));
                var mmbRules = mmb.MemoryAcl.GetAccessRules(true, false, typeof(SecurityIdentifier));

                var rules = new[] { mcbRules, mmbRules };

                // assert
                foreach (var ruleset in rules)
                {
                    Assert.That(ruleset.Count, Is.EqualTo(2));

                    Assert.That(ruleset.Cast<AccessRule>().Any(r => r.IdentityReference == networkServiceSid));
                    Assert.That(ruleset.Cast<AccessRule>()
                        .Where(r => r.IdentityReference == networkServiceSid)
                        .Any(r => r.AccessControlType == AccessControlType.Allow));

                    Assert.That(ruleset.Cast<AccessRule>().Any(r => r.IdentityReference == self));
                    Assert.That(ruleset.Cast<AccessRule>()
                        .Where(r => r.IdentityReference == self)
                        .Any(r => r.AccessControlType == AccessControlType.Allow));
                }
            }
        }

        [Test]
        public void Manager_Adds_SafeMode_EnvironmentVariable_When_SafemodeOn()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.SafeMode).Returns(true);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.NotNull(dict[@"OpenCover_Profiler_SafeMode"]);
            Assert.AreEqual("1", dict[@"OpenCover_Profiler_SafeMode"]);
        }

        [Test]
        public void Manager_DoesNotAdd_SafeMode_EnvironmentVariable_When_SafemodeOff()
        {
            // arrange
            var dict = new StringDictionary();
            Container.GetMock<ICommandLine>().SetupGet(x => x.SafeMode).Returns(false);

            // act
            RunSimpleProcess(dict);

            // assert
            Assert.IsNull(dict[@"OpenCover_Profiler_SafeMode"]);
        }

        private void RunSimpleProcess(StringDictionary dict)
        {
            RunProcess(dict, standardMessageDataReady => { }, () => { });
        }

        private void RunProcess(StringDictionary dict, Action<EventWaitHandle> getStandardMessageDataReady, Action doExtraWork)
        {
            ProfilerManager.BufferWaitCount = 0;

            // arrange
            Instance.RunProcess(e =>
            {
                e(dict);

                var standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                getStandardMessageDataReady(standardMessageDataReady);

                standardMessageDataReady.Set();

                doExtraWork();

            }, Enumerable.Empty<string>().ToArray());

        }
    }
}
