using System;
using System.Collections.Specialized;
using System.Diagnostics;
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

        [Test, RequiresMTA, Repeat(10)]
        public void Manager_Handles_Profiler_StandardMessageEvent()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            var blockHandled = new ManualResetEvent(false);

            Container.GetMock<ICommunicationManager>()
                     .Setup(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()))
                     .Returns<IManagedMemoryBlock>(mmb =>
                         {
                             mmb.ProfilerHasResults.Reset();
                             blockHandled.Set();
                             return new byte[4];
                         });

            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, -5, Enumerable.Empty<string>()))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, -5, Enumerable.Empty<string>()))
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
                        blockHandled.WaitOne();
                    });
            }

            // assert
            Container.GetMock<ICommunicationManager>()
                .Verify(x => x.HandleMemoryBlock(It.IsAny<IManagedMemoryBlock>()), Times.Once());
        }

        [Test, RequiresMTA, Repeat(10)]
        public void Manager_Handles_Profiler_ResultsReady()
        {
            // arrange
            EventWaitHandle standardMessageReady = null;
            EventWaitHandle offloadComplete = new AutoResetEvent(false);

            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 1, Enumerable.Empty<string>()))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 1, Enumerable.Empty<string>()))
            {
                Container.GetMock<ICommunicationManager>()
                         .Setup(x => x.HandleCommunicationBlock(It.IsAny<IManagedCommunicationBlock>(), It.IsAny<Action<IManagedCommunicationBlock, IManagedMemoryBlock>>()))
                         .Callback<IManagedCommunicationBlock, Action<IManagedCommunicationBlock, IManagedMemoryBlock>>((_, offload) =>
                         {
                             standardMessageReady.Reset();
                             mcb.ProfilerRequestsInformation.Reset();

                             offload(mcb, mmb);
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

        [Test]
        public void Manager_Sets_Service_ACLs_On_Events()
        {
            // arrange
            var networkServiceSid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            var networkServiceAccount = networkServiceSid.Translate(typeof(NTAccount));
            var servicePrincipal = new[] { networkServiceAccount.ToString() };
            var self = WindowsIdentity.GetCurrent().User;

            // act
            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 1, servicePrincipal))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 1, servicePrincipal))
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
            using (var mcb = new MemoryManager.ManagedCommunicationBlock("Local", _key, 100, 1, servicePrincipal))
            using (var mmb = new MemoryManager.ManagedMemoryBlock("Local", _key, 100, 1, servicePrincipal))
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

        private void RunSimpleProcess(StringDictionary dict)
        {
            RunProcess(dict, standardMessageDataReady => { }, () => { });
        }

        private void RunProcess(StringDictionary dict, Action<EventWaitHandle> getStandardMessageDataReady, Action doExtraWork)
        {
            // arrange
            EventWaitHandle standardMessageDataReady = null;

            Instance.RunProcess(e =>
            {
                e(dict);

                standardMessageDataReady = new EventWaitHandle(false, EventResetMode.ManualReset,
                    @"Local\OpenCover_Profiler_Communication_SendData_Event_" + dict[@"OpenCover_Profiler_Key"] + "-1");

                getStandardMessageDataReady(standardMessageDataReady);

                standardMessageDataReady.Set();

                doExtraWork();

            }, Enumerable.Empty<string>().ToArray());

        }
    }
}
