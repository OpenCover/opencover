using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Filtering;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class FilterTests
    {
        #region TestData for AddFilter tests

        public class AssemblyClassData
        {
            public string AssemblyClass { get; set; }
            public string AssemblyResult { get; set; }
            public string ClassResult { get; set; }
            public FilterType FilterTypeResult { get; set; }
        }

#pragma warning disable 169

        private readonly string[] _invalidAssemblyClassPairs = { "Garbage", "+[]", "-[ ]", "[ ", " ]", "+[]]", "-[][", @"-[\]", @"+[X]\", "-[X]]", "+[X][" };

        private readonly AssemblyClassData[] _assemblyClassPairs =
                                                              {
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "+[System]Console",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "+[My App]Namespace",
                                                                          AssemblyResult = "My App",
                                                                          ClassResult = "Namespace",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "+[System]",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "-[System.*]Console",
                                                                          AssemblyResult = @"System\..*",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Exclusion, 
                                                                      },
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "+[System]Console.*",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = @"Console\..*",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData
                                                                      {
                                                                          AssemblyClass = "-[System.*]Console.*",
                                                                          AssemblyResult = @"System\..*",
                                                                          ClassResult = @"Console\..*",
                                                                          FilterTypeResult = FilterType.Exclusion, 
                                                                      }
                                                              };
#pragma warning restore 169   
        #endregion

        [Test]
        public void AddFilter_ThrowsException_WhenInvalid_AssemblyClassPair(
            [ValueSource("_invalidAssemblyClassPairs")]string assemblyClassPair)
        {
            // arrange
            var filter = new Filter();

            // act/assert
            Assert.Catch<InvalidOperationException>(() => filter.AddFilter(assemblyClassPair), 
                "'{0}' should be invalid", assemblyClassPair);     
        }

        [Test]
        public void AddFilter_Adds_ValidAssemblyClassPair(
            [ValueSource("_assemblyClassPairs")]AssemblyClassData assemblyClassPair)
        {
            // arrange
            var filter = new Filter();

            // act
            filter.AddFilter(assemblyClassPair.AssemblyClass);

            // assert
            Assert.AreEqual(1, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ? 
                filter.InclusionFilters.Count : filter.ExclusionFilters.Count);

            Assert.AreEqual(assemblyClassPair.AssemblyResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilters[0].AssemblyName : filter.ExclusionFilters[0].AssemblyName);

            Assert.AreEqual(assemblyClassPair.ClassResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilters[0].ClassName : filter.ExclusionFilters[0].ClassName);
        }

        #region Test Data for UseAssembly tests

        public class UseAssemblyData
        {
            public List<string> Filters { get; set; }
            public string Assembly { get; set; }
            public bool ExpectedResult { get; set; }
        }

        private readonly UseAssemblyData[] _useAssemblyData = 
                                                         {
                                                             new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string>(0),
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"-[System.*]R*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"-[System.*]*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"+[System.*]*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"-[mscorlib]*", "-[System.*]*", "+[*]*"},
                                                                     Assembly = "mscorlib",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"+[XYZ]*"},
                                                                     Assembly = "XYZ",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData
                                                                 {
                                                                     Filters = new List<string> {"+[XYZ]*"},
                                                                     Assembly = "XYZA",
                                                                     ExpectedResult = false
                                                                 }
                                                         };
        #endregion

        [Test]
        public void UseAssembly_Tests(
            [ValueSource("_useAssemblyData")]UseAssemblyData data)
        {
            // arrange
            var filter = new Filter();
            data.Filters.ForEach(filter.AddFilter);

            // act
            var result = filter.UseAssembly(data.Assembly);

            // result
            Assert.AreEqual(data.ExpectedResult, result, 
                "Filter: '{0}' Assembly: {1} => Expected: {2}", 
                string.Join(",", data.Filters), data.Assembly, data.ExpectedResult);
        }

        #region Test Data for InstrumentClass tests

        public class InstrumentClassData
        {
            public List<string> Filters { get; set; }
            public string Assembly { get; set; }
            public string Class { get; set; }
            public bool ExpectedResult { get; set; }
        }

        private readonly InstrumentClassData[] _instrumentClassData =
                                                                 {
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]A*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ*]A*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]A*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]*Class"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]*Name"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[XYZ]*space.C*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[XYZ*]*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[XYZ]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[*]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[X*Z]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[XYZ]*Class"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"-[XYZ]*Unknown"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[*]*"},
                                                                             Assembly = "",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData
                                                                         {
                                                                             Filters = new List<string> {"+[*]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "",
                                                                             ExpectedResult = false
                                                                         }

                                                                 };
        #endregion

        [Test]
        public void InstrumentClass_Tests(
            [ValueSource("_instrumentClassData")]InstrumentClassData data)
        {
            //// arrange
            var filter = new Filter();
            data.Filters.ForEach(filter.AddFilter);

            // act
            var result = filter.InstrumentClass(data.Assembly, data.Class);

            // result
            Assert.AreEqual(data.ExpectedResult, result,
               "Filter: '{0}' Assembly: {1} Class: {2} => Expected: {3}", 
               string.Join(",", data.Filters), data.Assembly, data.Class, data.ExpectedResult);
        }

        [Test]
        public void AddAttributeExclusionFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(new []{ null, "" });

            Assert.AreEqual(1, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedAttributes[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void Entity_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter();
            var entity = new Mock<IMemberDefinition>();

            Assert.IsFalse(filter.ExcludeByAttribute(entity.Object));
        }

        [Test]
        public void AddFileExclusionFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedFiles[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void AddTestFileFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(null);

            Assert.AreEqual(0, filter.TestFiles.Count);
        }

        [Test]
        public void AssemblyIsIncludedForTestMethodGatheringWhenFilterMatches()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new []{"A*"});

            Assert.IsTrue(filter.UseTestAssembly("ABC.dll"));
            Assert.IsFalse(filter.UseTestAssembly("XYZ.dll"));
            Assert.IsFalse(filter.UseTestAssembly(""));
        }

        [Test]
        public void AddTestFileFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.TestFiles.Count);
        }

        [Test]
        public void AddTestFileFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new[] { ".*" });

            Assert.IsTrue(filter.TestFiles[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter();

            Assert.IsFalse(filter.ExcludeByFile("xyz.cs"));
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_File_Not_Supplied()
        {
            var filter = new Filter();

            Assert.IsFalse(filter.ExcludeByFile(""));
        }

        [Test]
        public void File_Is_Not_Excluded_If_Does_Not_Match_Filter()
        {
            var filter = new Filter();
            filter.AddFileExclusionFilters(new[]{"XXX.*"});

            Assert.IsFalse(filter.ExcludeByFile("YYY.cs"));
        }

        [Test]
        public void File_Is_Excluded_If_Matches_Filter()
        {
            var filter = new Filter();
            filter.AddFileExclusionFilters(new[] { "XXX.*" });

            Assert.IsTrue(filter.ExcludeByFile("XXX.cs"));
        }

        [Test]
        public void Can_Identify_Excluded_Methods()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof (Samples.Concrete).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods)
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter) continue;
                Assert.True(filter.ExcludeByAttribute(methodDefinition));                
            }

        }

        [Test]
        public void Can_Identify_Excluded_Properties()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Concrete).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var propertyDefinition in type.Properties)
            {
                Assert.True(filter.ExcludeByAttribute(propertyDefinition));
            }
        }

        [Test]
        public void Can_Identify_Excluded_Anonymous_Issue99()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Anonymous).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Anonymous).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods.Where(x=>x.Name.Contains("EXCLUDE")))
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter || methodDefinition.IsConstructor) continue;
                Assert.True(filter.ExcludeByAttribute(methodDefinition), "failed to execlude {0}", methodDefinition.Name);
            }
        }

        [Test]
        public void Can_Identify_Included_Anonymous_Issue99()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Anonymous).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Anonymous).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods.Where(x => x.Name.Contains("INCLUDE")))
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter || methodDefinition.IsConstructor) continue;
                Assert.False(filter.ExcludeByAttribute(methodDefinition), "failed to include {0}", methodDefinition.Name);
            }
        }

        [Test]
        public void Handles_Issue117()
        {
            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            var mockDefinition = new Mock<IMemberDefinition>();

            mockDefinition.SetupGet(x => x.HasCustomAttributes).Returns(true);
            mockDefinition.SetupGet(x => x.CustomAttributes).Returns(new Collection<CustomAttribute>());
            mockDefinition.SetupGet(x => x.Name).Returns("<>f_ddd");
            mockDefinition.SetupGet(x => x.DeclaringType).Returns(new TypeDefinition("","f_ddd", TypeAttributes.Public));

            Assert.DoesNotThrow(() => filter.ExcludeByAttribute(mockDefinition.Object));
        }

        [Test]
        public void CanIdentify_AutoImplementedProperties()
        {
            // arrange
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);
            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.DeclaredMethodClass).FullName);

            // act/assert
            var filter = new Filter();
            var wasTested = false;
            foreach (var methodDefinition in type.Methods
                .Where(x => x.IsGetter || x.IsSetter).Where(x => x.Name.EndsWith("AutoProperty")))
            {
                wasTested = true;
                Assert.IsTrue(filter.IsAutoImplementedProperty(methodDefinition));
            }
            Assert.IsTrue(wasTested);

            wasTested = false;
            foreach (var methodDefinition in type.Methods
                .Where(x => x.IsGetter || x.IsSetter).Where(x => x.Name.EndsWith("PropertyWithBackingField")))
            {
                wasTested = true;
                Assert.IsFalse(filter.IsAutoImplementedProperty(methodDefinition));
            }
            Assert.IsTrue(wasTested);
        }

        [Test]
        [TestCase("A1.B1", false)]
        [TestCase("A1.B2", true)]
        [TestCase("A1.B3", true)]
        [TestCase("A2.B3", false)]
        [TestCase("A.B", false)]
        public void CanHandle_AssemblyFilters_ExpressedAs_RegularExpressions(string assembly, bool canUse)
        {
            // arrange
            var filter = new Filter(true);
            filter.AddFilter(@"+[(A1\.B[23])]([CD]1.*)");

            // act

            // assert
            Assert.AreEqual(canUse, filter.UseAssembly(assembly));
        }

        [Test]
        [TestCase("A1", false)]
        [TestCase("C1", true)]
        [TestCase("D1", true)]
        [TestCase("D1234.ABC.Hope", true)]
        public void CanHandle_AssemblyClassFilters_ExpressedAs_RegularExpressions(string namespaceClass, bool canInstrument)
        {
            // arrange
            var filter = new Filter(true);
            filter.AddFilter(@"+[(A1\.B[23])]([CD]1.*)");

            // act

            // assert
            Assert.AreEqual(canInstrument, filter.InstrumentClass("A1.B2", namespaceClass));
        }

        [Test]
        public void Can_Identify_Excluded_Methods_UsingRegularExpressions()
        {
            // arrange
            var filter = new Filter(true);
            filter.AddAttributeExclusionFilters(new[] { ".*ExcludeMethodAttribute" });

            // act
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);
            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Concrete).FullName);

            // assert
            foreach (var methodDefinition in type.Methods.Where(methodDefinition => !methodDefinition.IsSetter && !methodDefinition.IsGetter))
            {
                Assert.True(filter.ExcludeByAttribute(methodDefinition));
            }
        }

        [Test]
        public void File_Is_Excluded_If_Matches_Filter_UsingRegularExpressions()
        {
            // arrange
            var filter = new Filter(true);
            filter.AddFileExclusionFilters(new[] { @"XXX\..*" });

            // act, assert
            Assert.IsTrue(filter.ExcludeByFile("XXX.cs"));
        }

        [Test]
        [TestCase(new[] { "-target:t" }, false)]
        [TestCase(new[] { "-target:t", "-nodefaultfilters" }, false)]
        [TestCase(new[] { "-target:t", "-nodefaultfilters", "-filter:+[*]*" }, true)]
        [TestCase(new[] { "-target:t", "-regex" }, false)]
        [TestCase(new[] { "-target:t", "-nodefaultfilters", "-regex", "-filter:+[(.*)](.*)" }, true)]
        public void Can_BuildFilter_From_CommandLine(string[] commandLine, bool matchAssembly)
        {
            var filter = Filter.BuildFilter(new CommandLineParser(commandLine).Do(_ => _.ExtractAndValidateArguments()));
            Assert.IsNotNull(filter);
            Assert.AreEqual(matchAssembly, filter.UseAssembly("System"));
        }
    }
}
