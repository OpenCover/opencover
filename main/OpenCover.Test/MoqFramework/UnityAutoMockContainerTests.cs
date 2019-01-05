using System;
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
