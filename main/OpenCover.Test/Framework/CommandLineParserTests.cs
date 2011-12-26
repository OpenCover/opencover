using System;
using NUnit.Framework;
using OpenCover.Framework;
using log4net.Core;

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
            Assert.IsFalse(parser.Service);
            Assert.IsFalse(parser.ShowUnvisited);
            Assert.IsFalse(parser.MergeByHash);
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
            Assert.AreEqual(0, parser.ReturnCodeOffset);
        }

        [Test]
        public void HandlesReturnTargetCodeArgument_WithValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-returntargetcode:100", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.ReturnTargetCode);
            Assert.AreEqual(100, parser.ReturnCodeOffset);
        }

        [Test]
        public void InvalidReturnTargetCodeArgumentValue_ThrowsException()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-returntargetcode:wibble", RequiredArgs });

            // act, assert
            Assert.Throws<InvalidOperationException>(parser.ExtractAndValidateArguments);
        }

        [Test]
        public void HandlesExcludeByAttributeArgument_WithValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-excludebyattribute:wibble", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(1, parser.AttributeExclusionFilters.Count);
            Assert.AreEqual("wibble", parser.AttributeExclusionFilters[0]);
        }

        [Test]
        public void HandlesExcludeByAttributeArgument_WithMultipleValues()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-excludebyattribute:wibble;wobble;woop", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(3, parser.AttributeExclusionFilters.Count);
            Assert.AreEqual("wibble", parser.AttributeExclusionFilters[0]);
            Assert.AreEqual("wobble", parser.AttributeExclusionFilters[1]);
            Assert.AreEqual("woop", parser.AttributeExclusionFilters[2]);
        }

        [Test]
        public void HandlesExcludeByFileArgument_WithValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-excludebyfile:wibble", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(1, parser.FileExclusionFilters.Count);
            Assert.AreEqual("wibble", parser.FileExclusionFilters[0]);
        }

        [Test]
        public void HandlesExcludeByFileArgument_WithMultipleValues()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-excludebyfile:wibble;wobble;woop", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(3, parser.FileExclusionFilters.Count);
            Assert.AreEqual("wibble", parser.FileExclusionFilters[0]);
            Assert.AreEqual("wobble", parser.FileExclusionFilters[1]);
            Assert.AreEqual("woop", parser.FileExclusionFilters[2]);
        }
        
        [Test]
        public void HandlesLogArgument_ValidValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[] {"-log:info", RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(parser.LogLevel, Level.Info);
        }

        [Test]
        public void HandlesLogArgument_WithInvalidValue_ThrowsException()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-log:wibble", RequiredArgs });

            // act
            Assert.Throws<InvalidOperationException>(parser.ExtractAndValidateArguments);
        }

        [Test]
        public void DetectsServiceArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-service", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Service);
        }

    }
}