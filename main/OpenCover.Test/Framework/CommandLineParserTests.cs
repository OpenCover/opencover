using System;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Common;

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
            var parser = new CommandLineParser(new[]{string.Empty});

            // assert
            Assert.IsFalse(parser.Register);
            Assert.IsFalse(parser.UserRegistration);
            Assert.IsFalse(parser.NoDefaultFilters);
        }

        [Test]
        public void ThrowsExceptionWhenArgumentUnrecognised()
        {
            // arrange
            var parser = new CommandLineParser(new[]{"-bork"});

            // act
            Assert.Throws<InvalidOperationException>(parser.ExtractAndValidateArguments);

            // assert
        }

        [Test]
        public void HandlesTheRegisterArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-register" , RequiredArgs});

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
            var parser = new CommandLineParser(new[]{"-register:user" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.IsTrue(parser.UserRegistration);
        }

        [Test]
        public void HandlesTheTargetArgumentThrowsExceptionWithMissingValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-target"});

            // act
            var ex = Assert.Catch<Exception>(parser.ExtractAndValidateArguments);
            
            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesTheTargetArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-target:XXX"});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.Target);
        }

        [Test]
        public void HandlesTheTargetDirArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-targetdir:XXX" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.TargetDir);
        }

        [Test]
        public void HandlesTheTargetArgsArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-targetargs:XXX" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX", parser.TargetArgs);
        }

        [Test]
        public void HandlesTheOutputArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-output:ZYX" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("ZYX", parser.OutputFile);
        }

        [Test]
        public void HandlesTheUsageArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-?"});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(true, parser.PrintUsage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(parser.Usage()));
        }

        [Test]
        public void HandlesAnInvalidTypeArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-type:method,boris" , RequiredArgs});

            // act
            var ex = Assert.Catch<Exception>(parser.ExtractAndValidateArguments);

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesNoDefaultFiltersArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-nodefaultfilters" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.NoDefaultFilters);
        }

        [Test]
        public void HandlesFilterArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-filter:XYZ ABC", RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
        }

        [Test]
        public void HandlesMergeByHashArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-mergebyhash", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.MergeByHash);
        }

        [Test]
        public void HandlesShowUnvisitedArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-showunvisited", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.ShowUnvisited);
        }

        [Test]
        public void HandlesReturnTargetCodeArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-returntargetcode", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.ReturnTargetCode);
        }

    }
}