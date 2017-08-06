using System;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Target;

namespace TargetTests
{
    [TestClass]
    public class TimeWrapperTests
    {
        [TestMethod]
        public void UnshimedCurrentTimeReturnsExpectedTime()
        {
            var wrapper = new TimeWrapper();
            var now = wrapper.CurrentTime;
            
            Assert.IsTrue(now - DateTime.Now < new TimeSpan(0,0,2));
        }

        [TestMethod]
        public void UnshimedCurrentUtcTimeReturnsExpectedTime()
        {
            var wrapper = new TimeWrapper();
            var now = wrapper.CurrentUtcTime;

            Assert.IsTrue(now - DateTime.UtcNow < new TimeSpan(0, 0, 2));
        }

        [TestMethod]
        public void ShimedCurrentTimeReturnsExpectedTime()
        {
            var wrapper = new TimeWrapper();
            var expected = new DateTime(2032, 1, 1);
            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.NowGet = () => expected;
                var now = wrapper.CurrentTime;
                Assert.AreEqual(expected, now);
            }
        }

        [TestMethod]
        public void ShimedCurrentUtcTimeReturnsExpectedTime()
        {
            var wrapper = new TimeWrapper();
            var expected = new DateTime(2032, 1, 1);
            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => expected;
                var now = wrapper.CurrentUtcTime;
                Assert.AreEqual(expected, now);
            }
        }
    }
}
