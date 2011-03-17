using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework.Service;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Service
{
    [TestFixture]
    public class ProfilerCommunicationTests :
        UnityAutoMockContainerBase<IProfilerCommunication, ProfilerCommunication>
    {
        [Test]
        public void ShouldTrackAssembly_Adds_AssemblyToModel_If_FilterUseAssembly_Returns_True()
        {
            
            Assert.Fail();
        }

        [Test]
        public void ShouldTrackAssembly_DoesntAdd_AssemblyToModel_If_FilterUseAssembly_Returns_False()
        {

            Assert.Fail();
        }
    }
}
