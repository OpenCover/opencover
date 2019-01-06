using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using OpenCover.Extensions;
using OpenCover.Extensions.Strategy;
using OpenCover.Framework.Strategy;

namespace OpenCover.Test.Extensions
{
    [TestFixture]
    public class RegisterStrategiesModuleTests
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new RegisterStrategiesModule());

            _container = builder.Build();
        }

        [Test]
        public void Extension_Registers_TestStrategies()
        {
            var strategies = _container.Resolve<IEnumerable<ITrackedMethodStrategy>>().ToList();

            Assert.IsTrue(strategies.Any(s => s is TrackNUnitTestMethods));
            Assert.IsTrue(strategies.Any(s => s is TrackMSTestTestMethods));
            Assert.IsTrue(strategies.Any(s => s is TrackXUnitTestMethods));
        }
    }
}
