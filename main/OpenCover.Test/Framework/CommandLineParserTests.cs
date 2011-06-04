using System;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Common;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class CommandLineParserTests
    {
        private const string RequiredArgs = " -target:Required";

        [Test]
        public void ParserHasKnownDefaultArguments()
        {
            // arrange
    
            // act
            var parser = new CommandLineParser(string.Empty);

            // assert
            Assert.IsFalse(parser.Register);
            Assert.IsFalse(parser.UserRegistration);
            Assert.IsFalse(parser.NoDefaultFilters);
        }

        [Test]
        public void ThrowsExceptionWhenArgumentUnrecognised()
        {
            // arrange
            var parser = new CommandLineParser("-bork");

            // act
            Assert.Throws<InvalidOperationException>(() => parser.ExtractAndValidateArguments());

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

        [Test]
        public void HandlesTheOutputArgumentWithSuppliedValue()
        {
            // arrange  
            var parser = new CommandLineParser("-output:ZYX" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("ZYX", parser.OutputFile);
        }

        [Test]
        public void HandlesTheUsageArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-?");

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(true, parser.PrintUsage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(parser.Usage()));
        }

        [Test]
        public void HandlesTheArchitectureArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-arch:32" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(Architecture.Arch32, parser.Architecture);
        }

        [Test]
        public void HandlesBadArchitectureArgumentNumber()
        {
            // arrange  
            var parser = new CommandLineParser("-arch:128" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesBadArchitectureArgumentAlt()
        {
            // arrange  
            var parser = new CommandLineParser("-arch:arch128" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesTheTypeArgumentSingle()
        {
            // arrange  
            var parser = new CommandLineParser("-type:method" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(CoverageType.Method, parser.CoverageType);
        }

        [Test]
        public void HandlesTheTypeArgumentMultiple()
        {
            // arrange  
            var parser = new CommandLineParser("-type:method, sequence" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(CoverageType.Method | CoverageType.Sequence, parser.CoverageType);
        }

        [Test]
        public void HandlesAnInvalidTypeArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-type:method,boris" + RequiredArgs);

            // act
            var ex = Assert.Catch<Exception>(() => parser.ExtractAndValidateArguments());

            // assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void HandlesNoDefaultFiltersArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-nodefaultfilters" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.NoDefaultFilters);
        }

        [Test]
        public void HandlesFilterArgument()
        {
            // arrange  
            var parser = new CommandLineParser("-filter:XYZ ABC" + RequiredArgs);

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
        }


    }
}