using System;
using System.Linq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
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
            Assert.AreEqual(Registration.Normal, parser.Registration);
            Assert.IsFalse(parser.NoDefaultFilters);
            Assert.IsFalse(parser.Service);
            Assert.IsFalse(parser.ShowUnvisited);
            Assert.IsFalse(parser.MergeByHash);
            Assert.IsFalse(parser.EnablePerformanceCounters);
            Assert.IsFalse(parser.TraceByTest);
            Assert.IsFalse(parser.SkipAutoImplementedProperties);
            Assert.IsFalse(parser.RegExFilters);
            Assert.IsFalse(parser.PrintVersion);
            Assert.AreEqual(new TimeSpan(0, 0, 30), parser.ServiceStartTimeout);
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
            Assert.AreEqual(Registration.Normal, parser.Registration);
        }

        [Test]
        public void HandlesTheRegisterArgumentWithUserValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[]{"-register:User" , RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.AreEqual(Registration.User, parser.Registration);
        }

        [Test]
        public void HandlesTheRegisterArgumentWithPath32Value()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-register:path32", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.AreEqual(Registration.Path32, parser.Registration);
        }

        [Test]
        public void HandlesTheRegisterArgumentWithPath64Value()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-register:path64", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.Register);
            Assert.AreEqual(Registration.Path64, parser.Registration);
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
            var parser = new CommandLineParser(new[]{"-filter:+[XYZ]ABC -[XYZ]ABC*", RequiredArgs});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
            Assert.AreEqual("+[XYZ]ABC", parser.Filters[0]);
            Assert.AreEqual("-[XYZ]ABC*", parser.Filters[1]);
        }

        [Test]
        public void HandlesFilterArgumentsWithSpacesInNamespace()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-filter:+[XY Z]ABC -[XY Z*]ABC", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
            Assert.AreEqual("+[XY Z]ABC", parser.Filters[0]);
            Assert.AreEqual("-[XY Z*]ABC", parser.Filters[1]);
        }

        [Test]
        public void HandlesFilterArgumentsWithEmptyArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-filter:  ", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(0, parser.Filters.Count);
           
        }

        [Test]
        public void HandlesFilterFileArgumentsWithEmptyArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-filterfile:XYZABC.LOG", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XYZABC.LOG", parser.FilterFile);

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
        public void HandlesThresholdArgument_WithValue()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-threshold:127", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(127, parser.Threshold);
        }

        [Test]
        public void InvalidThresholdArgumentValue_ThrowsException()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-threshold:wibble", RequiredArgs });

            // assert
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
        public void HandlesCoverByTestArgument_WithMultipleValues()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-coverbytest:wibble;wobble;woop", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(3, parser.TestFilters.Count);
            Assert.AreEqual("wibble", parser.TestFilters[0]);
            Assert.AreEqual("wobble", parser.TestFilters[1]);
            Assert.AreEqual("woop", parser.TestFilters[2]);
            Assert.IsTrue(parser.TraceByTest);
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

        [Test]
        public void DetectsOldStyleArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-oldstyle", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.OldStyleInstrumentation);
        }

        [Test]
        public void Detects_EnablePerformanceCounters_Argument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-enableperformancecounters", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.EnablePerformanceCounters);
        }

        [Test]
        public void ExtractsHideSkipped_Handles_Single_Arg()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:File", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(1, parser.HideSkipped.Count);
            Assert.AreEqual(SkippedMethod.File, parser.HideSkipped[0]);
        }

        [Test]
        public void ExtractsHideSkipped_Handles_Multiple_Arg()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:File;Filter;MissingPdb;Attribute", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(4, parser.HideSkipped.Distinct().Count());
            Assert.AreEqual(SkippedMethod.File, parser.HideSkipped[0]);
            Assert.AreEqual(SkippedMethod.Filter, parser.HideSkipped[1]);
            Assert.AreEqual(SkippedMethod.MissingPdb, parser.HideSkipped[2]);
            Assert.AreEqual(SkippedMethod.Attribute, parser.HideSkipped[3]);
        }

        [Test]
        public void ExtractsHideSkipped_RejectsUnexpected()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:File;wibble", RequiredArgs });

            // act
            Assert.Throws<InvalidOperationException>(parser.ExtractAndValidateArguments);
        }

        [Test]
        public void ExtractsHideSkipped_ConvertsAll()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:All", RequiredArgs });

            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(5, parser.HideSkipped.Distinct().Count());
        }

        [Test]
        public void ExtractsHideSkipped_Merges_AllFile()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:All;File", RequiredArgs });

            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(5, parser.HideSkipped.Distinct().Count());
        }

        [Test]
        public void ExtractsHideSkipped_Merges_FileFile()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped:File;File", RequiredArgs });

            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(1, parser.HideSkipped.Distinct().Count());
        }

        [Test]
        public void ExtractsHideSkipped_DefaultsToAll()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-hideskipped", RequiredArgs });

            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(5, parser.HideSkipped.Distinct().Count());
        }

        [Test]
        public void Extracts_SkipAutoImplementedProperties()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-skipautoprops", RequiredArgs });

            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.SkipAutoImplementedProperties);
        }
        
        [Test]
        public void HandlesTheTargetArgsArgumentWithSuppliedValueWithMultipleTimes()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-targetargs:XXX", "-targetargs:YYY", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("XXX YYY", parser.TargetArgs);
        }

        [Test]
        public void HandlesTheOutputArgumentWithSuppliedValueWithMultipleTimes()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-output:ZYX", "-output:XYZ", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual("ZYX XYZ", parser.OutputFile);
        }

        [Test]
        public void HandlesFilterArgumentWithMultipleTimes()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-filter:+[XYZ]ABC", "-filter:-[XYZ]ABC*", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
            Assert.AreEqual("+[XYZ]ABC", parser.Filters[0]);
            Assert.AreEqual("-[XYZ]ABC*", parser.Filters[1]);
        }

        [Test]
        public void HandlesFilterArgumentsWithSpacesInNamespaceWithMultipleTimes()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-filter:+[XY Z]ABC", "-filter:-[XY Z*]ABC", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.AreEqual(2, parser.Filters.Count);
            Assert.AreEqual("+[XY Z]ABC", parser.Filters[0]);
            Assert.AreEqual("-[XY Z*]ABC", parser.Filters[1]);
        }

        [Test]
        public void HandlesRegExArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-regex", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.RegExFilters);
        }

        [Test]
        public void HandlesMergeOutputArgument()
        {
            // arrange  
            var parser = new CommandLineParser(new[] { "-mergeoutput", RequiredArgs });

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.MergeExistingOutputFile);
        }

        [Test]
        public void HandlesVersionArgument()
        {
            // arrange
            var parser = new CommandLineParser(new[] {"-version"});

            // act
            parser.ExtractAndValidateArguments();

            // assert
            Assert.IsTrue(parser.PrintVersion);
        }

        [Test]
        public void NoArguments_ThrowException()
        {
            // arrange
            var parser = new CommandLineParser(new string[0]);
            
            // act
            var thrownException = Assert.Throws<InvalidOperationException>(parser.ExtractAndValidateArguments);

            // assert
            Assert.That(thrownException.Message, Contains.Substring("target"));
            Assert.That(thrownException.Message, Contains.Substring("required"));
        }
    }
}
