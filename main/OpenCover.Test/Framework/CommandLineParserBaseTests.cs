using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class CommandLineParserBaseTests
    {
        // this allows testing of the abstract class
        class CommandLineParserStub : CommandLineParserBase
        {
            public CommandLineParserStub(string arguments)
                : base(arguments)
            {
            }
        }

        [Test]
        public void CanHandleNullArgument()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub(null);

            // assert
            Assert.AreEqual(0, parser.ArgumentCount);
        }

        [Test]
        public void CanHandleEmptyArgument()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub(String.Empty);

            // assert
            Assert.AreEqual(0, parser.ArgumentCount);
        }

        [Test]
        public void CanParseOneArgument()
        {
            // arrange
           
            // act
            var parser = new CommandLineParserStub("-arg");

            // assert
            Assert.AreEqual(1, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
        }

        [Test]
        public void CanParseManyArguments()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub("-arg -arg2");

            // assert
            Assert.AreEqual(2, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
            Assert.IsTrue(parser.HasArgument("arg2"));
        }

        [Test]
        public void CanParseOneArgumentWithValue()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub("-arg:value");

            // assert
            Assert.AreEqual(1, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
            Assert.AreEqual("value", parser.GetArgumentValue("arg"));
        }

        [Test]
        public void CanParseManyArgumentsWithValue()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub("-arg:value -arg1:value1");

            // assert
            Assert.AreEqual(2, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
            Assert.AreEqual("value", parser.GetArgumentValue("arg"));
            Assert.IsTrue(parser.HasArgument("arg1"));
            Assert.AreEqual("value1", parser.GetArgumentValue("arg1"));
        }

        [Test]
        public void CanParseOneArgumentWithEmptyValue()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub("-arg:");

            // assert
            Assert.AreEqual(1, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
            Assert.AreEqual(String.Empty, parser.GetArgumentValue("arg"));
        }

        [Test]
        public void CanParseMultipleArgumentsWithValues()
        {
            // arrange

            // act
            var parser = new CommandLineParserStub("-arg:value -arg1 -arg2:");

            // assert
            Assert.AreEqual(3, parser.ArgumentCount);
            Assert.IsTrue(parser.HasArgument("arg"));
            Assert.AreEqual("value", parser.GetArgumentValue("arg"));
            Assert.IsTrue(parser.HasArgument("arg1"));
            Assert.AreEqual(String.Empty, parser.GetArgumentValue("arg1"));
            Assert.IsTrue(parser.HasArgument("arg2"));
            Assert.AreEqual(String.Empty, parser.GetArgumentValue("arg2"));

        }
    }
}
