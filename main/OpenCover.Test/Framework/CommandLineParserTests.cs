using System;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class CommandLineParserTests
    {
        private const string RequiredArgs = "-target:Required";

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
            var parser = new CommandLineParser("-bork");

            // act
            Assert.Throws<InvalidOperationException>(() =>parser.ExtractAndValidateArguments());

            // assert
        }

        [Test]
        public void HandlesTheRegisterArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-register" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.IsFalse(parser.UserRegistration);
        }

        [Test]
        public void HandlesTheRegisterArgumentWithKnownValue()
        {
            // arrange  
            var parser = new CommandLineParser("-register:user" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.IsTrue(parser.UserRegistration);
        }

        [Test]
        public void HandlesTheHostArgumentWithDefault()
        {
            // arrange  
            var parser = new CommandLineParser("-host" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.HostOnly);
            Assert.AreEqual(20, parser.HostOnlySeconds);
        }

        [Test]
        public void HandlesTheHostArgumentWithKnownValue()
        {
            // arrange  
            var parser = new CommandLineParser("-host:15" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.HostOnly);
            Assert.AreEqual(15, parser.HostOnlySeconds);
        }

        [Test]
        public void HandlesTheHostArgumentThrowsExceptionWithBadValue()
        {
            // arrange  
            var parser = new CommandLineParser("-host:badvalue" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesThePortArgumentThrowsExceptionWithMissingValue()
        {
            // arrange  
            var parser = new CommandLineParser("-port" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());
            
            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesThePortArgumentWithKnownValue()
        {
            // arrange  
            var parser = new CommandLineParser("-port:9999" + RequiredArgs);
            
            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(9999, parser.PortNumber);
        }

        [Test]
        public void HandlesThePortArgumentThrowsExceptionWithBadValue()
        {
            // arrange  
            var parser = new CommandLineParser("-host:badvalue" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());
           
            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesTheTargetArgumentThrowsExceptionWithMissingValue()
        {
            // arrange  
            var parser = new CommandLineParser("-target");

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());
            
            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesTheTargetArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser("-target:XXX");

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.Target);
        }

        [Test]
        public void HandlesTheTargetDirArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser("-targetdir:XXX" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.TargetDir);
        }

        [Test]
        public void HandlesTheTargetArgsArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser("-targetargs:XXX" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.TargetArgs);
        }
    }
}