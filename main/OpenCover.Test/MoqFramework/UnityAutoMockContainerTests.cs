using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.AutoMocking.SelfTesting;
using NUnit.Framework;

namespace OpenCover.Test.MoqFramework
{
    [TestFixture]
    public class UnityAutoMockContainerTests
    {
        [NUnit.Framework.Test]
        public void RunAllSelfTests()
        {
            UnityAutoMockContainerFixture.RunAllTests(Console.WriteLine);
        }
    }
}
