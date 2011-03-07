using System;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class CommandLineParserTests
    {
        [Test]
        public void ParserHasKnownDefaultArguments()
        {
            // arrange
    
            // act
            var parser = new CommandLineParser(string.Empty);

            // assert
            Assert.IsFalse(parser.Register);
            Assert.IsFalse(parser.UserRegistration);
            Assert.IsFalse(parser.HostOnly);
            Assert.AreEqual(20, parser.HostOnlySeconds);
            Assert.AreEqual(0xBABE, parser.PortNumber);

        }

        [Test]
        public void ThrowsExceptionWhenArgumentUnrecognised()
        {
            // arrange
           
            // act
            Assert.Throws<InvalidOperationException>(() => new CommandLineParser("-bork"));

            // assert
        }

        [Test]
        public void HandlesTheRegisterArgument()
        {
            // arrange  

            // act
            var parser = new CommandLineParser("-register");

            // assert
            Assert.IsTrue(parser.Register);
            Assert.IsFalse(parser.UserRegistration);
        }

        [Test]
        public void HandlesTheRegisterArgumentWithKnownValue()
        {
            // arrange  

            // act
            var parser = new CommandLineParser("-register:user");

            // assert
            Assert.IsTrue(parser.Register);
            Assert.IsTrue(parser.UserRegistration);
        }

        [Test]
        public void HandlesTheHostArgumentWithDefault()
        {
            // arrange  

            // act
            var parser = new CommandLineParser("-host");

            // assert
            Assert.IsTrue(parser.HostOnly);
            Assert.AreEqual(20, parser.HostOnlySeconds);
        }

        [Test]
        public void HandlesTheHostArgumentWithKnownValue()
        {
            // arrange  

            // act
            var parser = new CommandLineParser("-host:15");

            // assert
            Assert.IsTrue(parser.HostOnly);
            Assert.AreEqual(15, parser.HostOnlySeconds);
        }

        [Test]
        public void HandlesTheHostArgumentThrowsExceptionWithBadValue()
        {
            // arrange  

            // act
            var ex = Assert.Catch<Exception>(() => new CommandLineParser("-host:badvalue"));

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesThePortArgumentThrowsExceptionWithMissingValue()
        {
            // arrange  

            // act
            var ex = Assert.Catch<Exception>(() => new CommandLineParser("-port"));

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesThePortArgumentWithKnownValue()
        {
            // arrange  

            // act
            var parser = new CommandLineParser("-port:9999");

            // assert
            Assert.AreEqual(9999, parser.PortNumber);
        }

        [Test]
        public void HandlesThePortArgumentThrowsExceptionWithBadValue()
        {
            // arrange  

            // act
            var ex = Assert.Catch<Exception>(() => new CommandLineParser("-host:badvalue"));

            // assert
            Assert.IsNotNull(ex);
        }
    }
}