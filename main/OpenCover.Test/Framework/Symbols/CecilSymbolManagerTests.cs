﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using OpenCover.Test.Samples;
using log4net;
using File = OpenCover.Framework.Model.File;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerTests
    {
        private CecilSymbolManager _reader;
        private string _location;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<IFilter> _mockFilter;
        private Mock<ILog> _mockLogger;
        private Mock<ITrackedMethodStrategyManager> _mockManager;
        
        [SetUp]
        public void Setup()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _mockFilter = new Mock<IFilter>();
            _mockLogger = new Mock<ILog>();
            _mockManager = new Mock<ITrackedMethodStrategyManager>();
            
            _location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll");

            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null);
            _reader.Initialise(_location, "OpenCover.Test");
        }

        [TearDown]
        public void Teardown()
        {
            //_reader.Dispose();
        }

        [Test]
        public void GetFiles_Returns_AllFiles_InModule()
        {
            //arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var files = _reader.GetFiles();

            //assert
            Assert.NotNull(files);
            Assert.AreNotEqual(0, files.GetLength(0));
        }

        [Test]
        public void GetInstrumentableTypes_Returns_AllTypes_InModule_ThatCanBeInstrumented()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.AreNotEqual(0, types.GetLength(0));
        }

        [Test]
        public void GetInstrumentableTypes_Does_Not_Return_Structs_With_No_Instrumentable_Code()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.IsNull(types.FirstOrDefault(x => x.FullName == typeof(NotCoveredStruct).FullName));
        }

        [Test]
        public void GetInstrumentableTypes_Does_Return_Structs_With_Instrumentable_Code()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.IsNotNull(types.FirstOrDefault(x => x.FullName == typeof(CoveredStruct).FullName));
        }

        [Test]
        public void GetMethodsForType_Returns_AllDeclared_ForType()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredMethodClass).FullName);


            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetSequencePointsForMethodToken()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredMethodClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetSequencePointsForToken(methods[0].MetadataToken);
            // assert

            Assert.IsNotNull(points);
        }

        [Test]
        public void GetBranchPointsForMethodToken_OneBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSingleDecision")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(0, points[0].Path);
            Assert.AreEqual(1, points[1].Path);
            Assert.AreEqual(19, points[0].StartLine);
            Assert.AreEqual(19, points[1].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_TwoBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasTwoDecisions")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[2].Offset, points[3].Offset);
            Assert.AreEqual(25, points[0].StartLine);
            Assert.AreEqual(26, points[2].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_CompleteIf()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasCompleteIf")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(32, points[0].StartLine);
            Assert.AreEqual(32, points[1].StartLine);
        }        

        [Test]
        public void GetBranchPointsForMethodToken_Switch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSwitch")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[0].Offset, points[2].Offset);            
            Assert.AreEqual(3, points[3].Path);
            
            Assert.AreEqual(44, points[0].StartLine);
            Assert.AreEqual(44, points[1].StartLine);
            Assert.AreEqual(44, points[2].StartLine);
            Assert.AreEqual(44, points[3].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_SwitchWithDefault()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSwitchWithDefault")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[0].Offset, points[2].Offset);
            Assert.AreEqual(3, points[3].Path);
            
            Assert.AreEqual(58, points[0].StartLine);
            Assert.AreEqual(58, points[1].StartLine);
            Assert.AreEqual(58, points[2].StartLine);
            Assert.AreEqual(58, points[3].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_SwitchWithBreaks()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSwitchWithBreaks")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[0].Offset, points[2].Offset);
            Assert.AreEqual(3, points[3].Path);

            Assert.AreEqual(74, points[0].StartLine);
            Assert.AreEqual(74, points[1].StartLine);
            Assert.AreEqual(74, points[2].StartLine);
            Assert.AreEqual(74, points[3].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_SwitchWithMultipleCases()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSwitchWithMultipleCases")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count()); // there's one branch generated for missing case = 2
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[0].Offset, points[2].Offset);
            Assert.AreEqual(points[0].Offset, points[3].Offset);
            Assert.AreEqual(3, points[3].Path);

            Assert.AreEqual(92, points[0].StartLine);
            Assert.AreEqual(92, points[1].StartLine);
            Assert.AreEqual(92, points[2].StartLine);
            Assert.AreEqual(92, points[3].StartLine);
        }

        [Test]
        public void GetBranchPointsForMethodToken_WithUsing()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasUsing")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(-1, points[0].StartLine);
            Assert.AreEqual(-1, points[1].StartLine);
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreNotEqual(points[0].Path, points[1].Path);
        }

        [Test]
        public void GetBranchPointsForMethodToken_WithFinallyAndSwitch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasTryFinallyWithTernary")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(128, points[0].StartLine);
            Assert.AreEqual(128, points[1].StartLine);
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreNotEqual(points[0].Path, points[1].Path);
        }

        [Test]
        public void GetBranchPointsForMethodToken_AssignsNegativeLineNumberToBranchesInMethodsThatHaveNoInstrumentablePoints()
        {
            /* 
             * Yes these actually exist - the compiler is very inventive
             * in this case for an anonymous class the compiler will dynamically create an Equals 'utility' method. 
             */
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName.Contains("f__AnonymousType"));
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::Equals")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            foreach (var branchPoint in points)
                Assert.AreEqual(-1, branchPoint.StartLine);
        }

        [Test]
        public void GetSequencePointsForToken_HandlesUnknownTokens()
        {
            // arrange

            // act
            var points = _reader.GetSequencePointsForToken(0);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());

        }

        [Test]
        public void ModulePath_Returns_Name_Of_Module()
        {
            // arrange, act, assert
            Assert.AreEqual(_location, _reader.ModulePath);
        }

        [Test]
        public void SourceAssembly_Returns_Null_On_Failure()
        {
            // arrange
            _reader.Initialise("", "");

            // act
            var val = _reader.SourceAssembly;

            // assert
            Assert.IsNull(val);    
        }

        [Test]
        public void GetComplexityForToken_HandlesUnknownTokens()
        {
            // arrange

            // act
            var complexity = _reader.GetCyclomaticComplexityForToken(0);

            // assert
            Assert.IsNotNull(complexity);
            Assert.AreEqual(0, 0);
        }

        [Test]
        public void GetComplexityForMethodToken_TwoBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(DeclaredConstructorClass).FullName).First();
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var complexity = _reader.GetCyclomaticComplexityForToken(methods.Where(x => x.Name.Contains("::HasTwoDecisions")).First().MetadataToken);

            // assert
            Assert.AreEqual(3, complexity);
        }

        [Test]
        public void AbstractPropertyGetters_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x=>x.IsGetter));
        }

        [Test]
        public void AbstractPropertySetters_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x => x.IsSetter));
        }

        [Test]
        public void AbstractMethods_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x => !x.IsGetter && !x.IsSetter && !x.IsConstructor));
        }

        [Test]
        public void GetSequencePointsFor_OverridePropertyGetter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(Concrete).FullName).First();
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetSequencePointsForToken(methods.First(x => x.IsGetter).MetadataToken);

            // assert
            Assert.IsNotNull(points);
#if DEBUG
            Assert.AreEqual(3, points.Count());
#else
            Assert.AreEqual(1, points.Count());
#endif

        }

        [Test]
        public void Can_Exclude_A_Class_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof (Concrete).MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<IMemberDefinition>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();

            Assert.True(types.Any());
            Assert.True(types.First(x => x.FullName == typeof(Concrete).FullName).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Class_By_An_Filter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((assemblyName, className) => className != typeof(Concrete).FullName);

            var types = _reader.GetInstrumentableTypes();

            Assert.True(types.Any());
            Assert.True(types.First(x => x.FullName == typeof(Concrete).FullName).SkippedDueTo == SkippedMethod.Filter);
        }

        [Test]
        public void Can_Exclude_A_Property_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("get_Name").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<IMemberDefinition>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::get_Name()")).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Ctor_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null).MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<IMemberDefinition>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::.ctor()")).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Method_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("Method").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<IMemberDefinition>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0] );

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::Method()")).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Method_By_An_FileFilter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("Method").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByFile(It.Is<string>(y => !string.IsNullOrWhiteSpace(y) && y.EndsWith("Samples.cs"))))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::Method()")).SkippedDueTo == SkippedMethod.File);
        }

        [Test]
        public void Can_Exclude_AutoImplmentedProperties()
        {
            // arrange
            var filter = new Filter();
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _mockFilter
                .Setup(x => x.IsAutoImplementedProperty(It.IsAny<MethodDefinition>()))
                .Returns<MethodDefinition>(x => filter.IsAutoImplementedProperty(x));

            _mockCommandLine.Setup(x => x.SkipAutoImplementedProperties).Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(DeclaredMethodClass).FullName);

            // act
            var methods = _reader.GetMethodsForType(target, new File[0]);

            // assert
            Assert.True(methods.Any());
            Assert.AreEqual(SkippedMethod.AutoImplementedProperty, methods.First(y => y.Name.EndsWith("AutoProperty()")).SkippedDueTo);
            Assert.AreEqual((SkippedMethod)0, methods.First(y => y.Name.EndsWith("PropertyWithBackingField()")).SkippedDueTo);
        }

        [Test]
        public void GetTrackedMethods_NoTrackedMethods_When_NoStrategies()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, _mockManager.Object);
            _reader.Initialise(_location, "OpenCover.Test");

            // act
            var methods = _reader.GetTrackedMethods();

            // assert
            Assert.IsFalse(methods.Any());
        }

        [Test]
        public void GetTrackedMethods_NoTrackedMethods_When_StrategiesFindNothing()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, _mockManager.Object);
            _reader.Initialise(_location, "OpenCover.Test");

            // act
            var methods = _reader.GetTrackedMethods();

            // assert
            Assert.IsFalse(methods.Any());
        }

        [Test]
        public void GetTrackedMethods_TrackedMethods_When_StrategiesMatch()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, _mockManager.Object);
            _reader.Initialise(_location, "OpenCover.Test");

            _mockManager.Setup(x => x.GetTrackedMethods(It.IsAny<string>()))
                .Returns(new[] { new TrackedMethod() });

            // act
            var methods = _reader.GetTrackedMethods();

            // assert
            Assert.AreEqual(1, methods.Count());
        }

        [Test]
        public void GetTrackedMethods_NoTrackedMethods_When_NoPDB()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, _mockManager.Object);
            _reader.Initialise(string.Empty, "OpenCover.Test");

            // act
            var methods = _reader.GetTrackedMethods();

            // assert
            Assert.IsNull(methods);
        }

        /// <summary>
        /// Issue #156
        /// </summary>
        [Test]
        public void GetTrackedMethods_Prepends_TargetDir_When_Assembly_NotFound()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, _mockManager.Object);
            _reader.Initialise(@"c:\OpenCover.Test.dll", "OpenCover.Test");
            _mockCommandLine.SetupGet(x => x.TargetDir).Returns(@"c:\temp");
            
            // act
            var methods = _reader.GetTrackedMethods();

            // assert
            _mockManager.Verify(x => x.GetTrackedMethods(@"c:\temp\OpenCover.Test.dll"));
        }

        [Test]
        public void SourceAssembly_DisplaysMessage_When_NoPDB()
        {
            // arrange
            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null);
            _reader.Initialise(string.Empty, "OpenCover.Test");
            _mockLogger.SetupGet(x => x.IsDebugEnabled).Returns(true);

            // act
            var source = _reader.SourceAssembly;

            // assert
            Assert.IsNull(source);
            _mockLogger.Verify(x => x.DebugFormat(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

    }
}