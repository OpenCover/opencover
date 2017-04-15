using System;
using Moq;
using NUnit.Framework;
using OpenCover.Samples.CS;
using OpenCover.Samples.Framework;
using OpenCover.Samples.IL;
using OpenCover.Samples.VB;

namespace OpenCover.Test.Integration
{
    [TestFixture(Description="Theses tests are targets used for integration testing")] 
    [Category("Integration")]
    [Explicit("Integration")]
    public class SimpleExceptionTests
    {
        [Test]
        public void TryFault_NoExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            var target = new TryFaultTarget(query.Object);
            
            // act
            target.TryFault();

            // assert
            query.Verify(x=>x.InFault(), Times.Never());
        }

        [Test]
        public void TryFault_ExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            query.Setup(x => x.ThrowException()).Callback(() => { throw new InvalidOperationException(); });
            var target = new TryFaultTarget(query.Object);

            // act
            Assert.Catch<InvalidOperationException>(target.TryFault);

            // assert
            query.Verify(x => x.InFault(), Times.Once());
        }

        [Test]
        public void TryFinally_NoExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            var target = new TryFinallyTarget(query.Object);

            // act
            target.TryFinally();

            // assert
            query.Verify(x => x.InFinally(), Times.Once());
        }

        [Test]
        public void TryFinally_ExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            query.Setup(x => x.ThrowException()).Callback(() => { throw new InvalidOperationException(); });
            var target = new TryFinallyTarget(query.Object);

            // act
            Assert.Catch<InvalidOperationException>(target.TryFinally);

            // assert
            query.Verify(x => x.InFinally(), Times.Once());
        }

        [Test]
        public void TryException_NoExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            var target = new TryExceptionTarget(query.Object);

            // act
            target.TryException();

            // assert
            query.Verify(x => x.InException(It.IsAny<Exception>()), Times.Never());
        }

        [Test]
        public void TryException_ExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            query.Setup(x => x.ThrowException()).Callback(() => { throw new InvalidOperationException(); });
            var target = new TryExceptionTarget(query.Object);

            // act
            Assert.Catch<InvalidOperationException>(target.TryException);

            // assert
            query.Verify(x => x.InException(It.IsAny<Exception>()), Times.Once());
        }

        [Test]
        public void TryFilter_NoExceptionThrown()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            var target = new TryFilterTarget(query.Object);

            // act
            target.TryFilter(0);

            // assert
            query.Verify(x => x.InFilter(), Times.Never());
        }

        [Test]
        public void TryFilter_ExceptionThrown_FilterFail()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            query.Setup(x => x.ThrowException()).Callback(() => { throw new InvalidOperationException(); });
            var target = new TryFilterTarget(query.Object);

            // act
            Assert.Catch<InvalidOperationException>(() => target.TryFilter(0));

            // assert
            query.Verify(x => x.InFilter(), Times.Never());
        }

        [Test]
        public void TryFilter_ExceptionThrown_FilterPass()
        {
            // arrange
            var query = new Mock<ITestExceptionQuery>();
            query.Setup(x => x.ThrowException()).Callback(() => { throw new InvalidOperationException(); });
            var target = new TryFilterTarget(query.Object);

            // act
            Assert.Catch<InvalidOperationException>(() => target.TryFilter(1));

            // assert
            query.Verify(x => x.InFilter(), Times.Once());
        }
    
    }
}
