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
    }
}