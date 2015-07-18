using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Strategy;
using OpenCover.Framework.Utility;
using log4net;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class BootstrapperTests
    {
        // arrange 
        private Mock<IFilter> _mockFilter;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<IPersistance> _mockPersistance;
        private Mock<IPerfCounters> _mockPerf;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            // arrange 
            _mockFilter = new Mock<IFilter>();
            _mockCommandLine = new Mock<ICommandLine>();
            _mockPersistance = new Mock<IPersistance>();
            _mockPerf = new Mock<IPerfCounters>();
            _mockLogger = new Mock<ILog>();
        }

        [Test]
        public void CanCreateProfilerCommunication()
        {
            using (var bootstrapper = new Bootstrapper(_mockLogger.Object))
            {
                bootstrapper.Initialise(_mockFilter.Object, _mockCommandLine.Object,
                                        _mockPersistance.Object, _mockPerf.Object);

                // act
                var instance = bootstrapper.Resolve<IProfilerCommunication>();

                // assert
                Assert.IsNotNull(instance);
            }
        }

        [Test]
        public void CanCreateInstrumentationModelBuilderFactory()
        {
            using (var bootstrapper = new Bootstrapper(_mockLogger.Object))
            {
                bootstrapper.Initialise(_mockFilter.Object, _mockCommandLine.Object,
                                        _mockPersistance.Object, _mockPerf.Object);

                // act
                var instance = bootstrapper.Resolve<IInstrumentationModelBuilderFactory>();

                // assert
                Assert.IsNotNull(instance);
            }
        }

        [Test]
        public void TrackedMethodStrategyManager_Is_Singleton()
        {
            using (var bootstrapper = new Bootstrapper(_mockLogger.Object))
            {
                bootstrapper.Initialise(_mockFilter.Object, _mockCommandLine.Object,
                                        _mockPersistance.Object, _mockPerf.Object);

                // act
                var instance1 = bootstrapper.Resolve<ITrackedMethodStrategyManager>();
                var instance2 = bootstrapper.Resolve<ITrackedMethodStrategyManager>();

                // assert
                Assert.IsNotNull(instance1);
                Assert.AreSame(instance1, instance2);
            }
        }

        [Test]
        public void MemoryManager_Is_Singleton()
        {
            using (var bootstrapper = new Bootstrapper(_mockLogger.Object))
            {
                bootstrapper.Initialise(_mockFilter.Object, _mockCommandLine.Object,
                                        _mockPersistance.Object, _mockPerf.Object);

                // act
                var instance1 = bootstrapper.Resolve<IMemoryManager>();
                var instance2 = bootstrapper.Resolve<IMemoryManager>();

                // assert
                Assert.IsNotNull(instance1);
                Assert.AreSame(instance1, instance2);
            }
        }
    }
}
