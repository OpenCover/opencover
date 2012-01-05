using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Symbols;
using log4net;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class BootstrapperTests
    {
        [Test]
        public void CanCreateProfilerCommunication()
        {
            // arrange 
            var mockFilter = new Mock<IFilter>();
            var mockCommandLine = new Mock<ICommandLine>();
            var mockPersistance = new Mock<IPersistance>();
            var mockLogger = new Mock<ILog>();

            var bootstrapper = new Bootstrapper(mockLogger.Object);
            bootstrapper.Initialise(mockFilter.Object, mockCommandLine.Object, mockPersistance.Object);

            // act
            var instance = bootstrapper.Container.Resolve(typeof(IProfilerCommunication), null);

            // assert
            Assert.IsNotNull(instance);

        }
    }
}
