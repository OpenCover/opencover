using System.Linq;
using NUnit.Framework;
using OpenCover.Framework.Strategy;

namespace OpenCover.Test.Framework.Strategy
{
    [TestFixture]
    public class TrackedMethodStrategyManagerTests
    {
        private ITrackedMethodStrategyManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new TrackedMethodStrategyManager();
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Dispose();
        }

        [Test]
        public void GetTrackedMethods_Locates_TestMethods_Within_TestAssembly()
        {
            // act
            var methods = _manager.GetTrackedMethods(typeof (TrackedMethodStrategyManagerTests).Assembly.Location);

            // assert
            Assert.AreNotEqual(0, methods.Count(x => x.Strategy == "MSTestTest"));
            Assert.AreNotEqual(0, methods.Count(x => x.Strategy == "NUnitTest"));
            Assert.AreNotEqual(0, methods.Count(x => x.Strategy == "xUnitTest"));
        }

        [Test]
        public void GetTrackedMethods_Assigns_UniqueIds_To_TrackedMethods()
        {
            // act
            var methods = _manager.GetTrackedMethods(typeof(TrackedMethodStrategyManagerTests).Assembly.Location);

            // assert
            Assert.AreEqual(1, methods[0].UniqueId);
            Assert.AreEqual(2, methods[1].UniqueId);
        }

        [Test]
        public void GetTrackedMethods_Can_Be_Executed_Multiple_Times()
        {
            // act
            var methods = _manager.GetTrackedMethods(typeof(TrackedMethodStrategyManagerTests).Assembly.Location);
            var methods2 = _manager.GetTrackedMethods(typeof(TrackedMethodStrategyManagerTests).Assembly.Location);

            // assert
            Assert.AreEqual(1, methods[0].UniqueId);
            Assert.AreEqual(methods.Length + 1, methods2[0].UniqueId);
        }

    }
}
