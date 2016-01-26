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

        public class FilterData
        {
            public FilterData()
            {
                ProcessResult = ".*";
            }

            public string FilterExpression { get; set; }
            public string ProcessResult { get; set; }
            public string AssemblyResult { get; set; }
            public string ClassResult { get; set; }
            public FilterType FilterTypeResult { get; set; }
        }

        private readonly string[] _invalidFilterExpressions =
        {
            "Garbage", "+[]", "-[ ]", "[ ", " ]", "+[]]", "-[][",
            @"-[\]", @"+[X]\", "-[X]]", "+[X][", "-<[*]*", "+>[*]*",
            "+<>[*]*", "-[*]", "-[]*", "-<*>[*]", "-<*>[]*", "-[\u00a0]*"
        };

        private readonly FilterData[] _filterExpressions =
        {
            new FilterData
            {
                FilterExpression = "+[My App]Namespace",
                AssemblyResult = "My App",
                ClassResult = "Namespace",
                FilterTypeResult = FilterType.Inclusion,
            },
            new FilterData
            {
                FilterExpression = "-[System.*]Console",
                AssemblyResult = @"System\..*",
                ClassResult = "Console",
                FilterTypeResult = FilterType.Exclusion,
            },
            new FilterData
            {
                FilterExpression = "+[System]Console.*",
                AssemblyResult = "System",
                ClassResult = @"Console\..*",
                FilterTypeResult = FilterType.Inclusion,
            },
            new FilterData
            {
                FilterExpression = "-[System.*]Console.*",
                AssemblyResult = @"System\..*",
                ClassResult = @"Console\..*",
                FilterTypeResult = FilterType.Exclusion,
            },
            new FilterData
            {
                FilterExpression = "+<*>[System.*]Console.*",
                AssemblyResult = @"System\..*",
                ClassResult = @"Console\..*",
                ProcessResult = ".*",
                FilterTypeResult = FilterType.Inclusion,
            },
            new FilterData
            {
                FilterExpression = "+[*]*",
                AssemblyResult = ".*",
                ClassResult = ".*",
                FilterTypeResult = FilterType.Inclusion,
            },
            new FilterData
            {
                FilterExpression = "+<*>[*]*",
                AssemblyResult = ".*",
                ClassResult = ".*",
                ProcessResult = ".*",
                FilterTypeResult = FilterType.Inclusion,
            },
            new FilterData
            {
                FilterExpression = "-<MyApplication.*>[*]*",
                AssemblyResult = ".*",
                ClassResult = ".*",
                ProcessResult = @"MyApplication\..*",
                FilterTypeResult = FilterType.Exclusion,
            }

        };
        #endregion

        [Test]
        public void AddFilter_ThrowsException_WhenInvalid_AssemblyClassPair(
            [ValueSource("_invalidFilterExpressions")]string assemblyClassPair)
        {
            // arrange
            var filter = new Filter(false);

            // act/assert
            Assert.Catch<ExitApplicationWithoutReportingException>(() => filter.AddFilter(assemblyClassPair),
                "'{0}' should be invalid", assemblyClassPair);
        }

        [Test]
        public void AddFilter_Adds_ValidAssemblyClassPair(
            [ValueSource("_filterExpressions")]FilterData assemblyClassPair)
        {
            // arrange
            var filter = new Filter(false);

            // act
            filter.AddFilter(assemblyClassPair.FilterExpression);

            // assert
            Assert.AreEqual(1, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilters.Count : filter.ExclusionFilters.Count);

            Assert.AreEqual(assemblyClassPair.ProcessResult,
                assemblyClassPair.FilterTypeResult == FilterType.Inclusion ? 
                filter.InclusionFilters[0].ProcessName
                    : filter.ExclusionFilters[0].ProcessName);

            Assert.AreEqual(assemblyClassPair.AssemblyResult,
                assemblyClassPair.FilterTypeResult == FilterType.Inclusion ? 
                filter.InclusionFilters[0].AssemblyName
                    : filter.ExclusionFilters[0].AssemblyName);

            Assert.AreEqual(assemblyClassPair.ClassResult, 
                assemblyClassPair.FilterTypeResult == FilterType.Inclusion ? 
                filter.InclusionFilters[0].ClassName
                : filter.ExclusionFilters[0].ClassName);
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
            var filter = new Filter(false);
            data.Filters.ForEach(filter.AddFilter);

            // act
            var result = filter.UseAssembly("processName.exe", data.Assembly);

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
            // arrange
            var filter = new Filter(false);
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
            var filter = new Filter(false);

            filter.AddAttributeExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter(false);

            filter.AddAttributeExclusionFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter(false);

            filter.AddAttributeExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedAttributes[0].IsMatchingExpression(".ABC"));
        }

        [Test]
        public void Entity_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter(false);
            var entity = new Mock<IMemberDefinition>();

            Assert.IsFalse(filter.ExcludeByAttribute(entity.Object));
        }

        [Test]
        public void AddFileExclusionFilters_HandlesNull()
        {
            var filter = new Filter(false);

            filter.AddFileExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter(false);

            filter.AddFileExclusionFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter(false);

            filter.AddFileExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedFiles[0].IsMatchingExpression(".ABC"));
        }

        [Test]
        public void AddTestFileFilters_HandlesNull()
        {
            var filter = new Filter(false);

            filter.AddTestFileFilters(null);

            Assert.AreEqual(0, filter.TestFiles.Count);
        }

        [Test]
        public void AssemblyIsIncludedForTestMethodGatheringWhenFilterMatches()
        {
            var filter = new Filter(false);

            filter.AddTestFileFilters(new[] { "A*" });

            Assert.IsTrue(filter.UseTestAssembly("ABC.dll"));
            Assert.IsFalse(filter.UseTestAssembly("XYZ.dll"));
            Assert.IsFalse(filter.UseTestAssembly(""));
        }

        [Test]
        public void AddTestFileFilters_Handles_Null_Elements()
        {
            var filter = new Filter(false);

            filter.AddTestFileFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.TestFiles.Count);
        }

        [Test]
        public void AddTestFileFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter(false);

            filter.AddTestFileFilters(new[] { ".*" });

            Assert.IsTrue(filter.TestFiles[0].IsMatchingExpression(".ABC"));
        }


        [Test]
        public void AddAttributeExclustionFilters_DoesNotWrap_ForRegexFilters()
        {
            var filter = new Filter(true);
            const string stringToMatch = "some string on the line before EXPRESSION some string after the expression";

            filter.AddAttributeExclusionFilters(new[] { "EXPRESSION" });

            var excludedAttributeRegexFilter = filter.ExcludedAttributes[0];
            Assert.IsTrue(excludedAttributeRegexFilter.IsMatchingExpression(stringToMatch));
        }

        [Test]
        public void AddFileExclustionFilters_DoesNotWrap_ForRegexFilters()
        {
            var filter = new Filter(true);
            const string stringToMatch = "some string on the line before EXPRESSION some string after the expression";

            filter.AddFileExclusionFilters(new[] { "EXPRESSION" });

            var excludedFileRegexFilter = filter.ExcludedFiles[0];
            Assert.IsTrue(excludedFileRegexFilter.IsMatchingExpression(stringToMatch));
        }

        [Test]
        public void AddTestFileFilters_DoesNotWrap_ForRegexFilters()
        {
            var filter = new Filter(true);
            const string stringToMatch = "some string on the line before EXPRESSION some string after the expression";

            filter.AddTestFileFilters(new[] { "EXPRESSION" });

            var excludedTestFileRegex = filter.TestFiles[0];
            Assert.IsTrue(excludedTestFileRegex.IsMatchingExpression(stringToMatch));
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter(false);

            Assert.IsFalse(filter.ExcludeByFile("xyz.cs"));
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_File_Not_Supplied()
        {
            var filter = new Filter(false);

            Assert.IsFalse(filter.ExcludeByFile(""));
        }

        [Test]
        public void File_Is_Not_Excluded_If_Does_Not_Match_Filter()
        {
            var filter = new Filter(false);
            filter.AddFileExclusionFilters(new[] { "XXX.*" });

            Assert.IsFalse(filter.ExcludeByFile("YYY.cs"));
        }

        [Test]
        public void File_Is_Excluded_If_Matches_Filter()
        {
            var filter = new Filter(false);
            filter.AddFileExclusionFilters(new[] { "XXX.*" });

            Assert.IsTrue(filter.ExcludeByFile("XXX.cs"));
        }

        [Test]
        public void Can_Identify_Excluded_Methods()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Concrete).FullName);

            var filter = new Filter(false);
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

            var filter = new Filter(false);
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

            var filter = new Filter(false);
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods.Where(x => x.Name.Contains("EXCLUDE")))
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

            var filter = new Filter(false);
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
            var filter = new Filter(false);
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            var mockDefinition = new Mock<IMemberDefinition>();

            mockDefinition.SetupGet(x => x.HasCustomAttributes).Returns(true);
            mockDefinition.SetupGet(x => x.CustomAttributes).Returns(new Collection<CustomAttribute>());
            mockDefinition.SetupGet(x => x.Name).Returns("<>f_ddd");
            mockDefinition.SetupGet(x => x.DeclaringType).Returns(new TypeDefinition("", "f_ddd", TypeAttributes.Public));

            Assert.DoesNotThrow(() => filter.ExcludeByAttribute(mockDefinition.Object));
        }

        [Test]
        public void Can_Identify_Excluded_Assemblies()
        {
            // arrange
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);
           
            // act
            var filter = new Filter(false);
            Assert.False(filter.ExcludeByAttribute(sourceAssembly));

            // assert
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeAssemblyAttribute" });
            Assert.True(filter.ExcludeByAttribute(sourceAssembly));
        }

        static IEnumerable<TypeDefinition> AllNestedTypes(TypeDefinition typeDefinition)
        {
            if (typeDefinition.NestedTypes == null) yield break;
            foreach (var nestedTypeDefinition in typeDefinition.NestedTypes)
            {
                yield return nestedTypeDefinition;
                foreach (var allNestedType in AllNestedTypes(nestedTypeDefinition))
                {
                    yield return allNestedType;
                }
            }
        }

        static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition module)
        {
            foreach (var typeDefinition in module.Types)
            {
                yield return typeDefinition;
                foreach (var allNestedType in AllNestedTypes(typeDefinition))
                {
                    yield return allNestedType;
                }
            }
        }
            
        [Test]
        [TestCase("Concrete")]
        [TestCase("InnerConcrete")]
        [TestCase("InnerInnerConcrete")]
        public void Can_Identify_Excluded_Types(string typeName)
        {
            // arrange
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            // act
            var filter = new Filter(false);
            var allTypes = AllTypes(sourceAssembly.MainModule);
            var typeDefinition = allTypes.First(x => x.Name == typeName);
            
            Assert.False(filter.ExcludeByAttribute(typeDefinition));

            // assert
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeClassAttribute" });
            foreach (var methodDefinition in typeDefinition.Methods)
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter) continue;
                Assert.True(filter.ExcludeByAttribute(methodDefinition));
            }
            Assert.True(filter.ExcludeByAttribute(typeDefinition));
            foreach (var nestedType in AllNestedTypes(typeDefinition))
            {
                Assert.True(filter.ExcludeByAttribute(nestedType));
            }
        }

        [Test]
        public void CanIdentify_AutoImplementedProperties()
        {
            // arrange
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);
            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.DeclaredMethodClass).FullName);

            // act/assert
            var filter = new Filter(false);
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
            Assert.AreEqual(canUse, filter.UseAssembly("processName.exe", assembly));
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
            Assert.AreEqual(matchAssembly, filter.UseAssembly("processName.exe", "System"));
        }

        [Test]
        // TestCase semantic changed!
        // first boolean is expected value when default filters disabled
        // second boolean is expected value when default filters enabled

        #region Initial test set
        [TestCase("+<*>[*]*", null, false, false)]
        [TestCase("-<*>[*]*", "process.exe", false, false)]
        [TestCase("-<pro*>[*]*", "process.exe", false, false)]
        [TestCase("-<*cess>[*]*", "process.exe", false, false)]
        [TestCase("+<*>[*]*", "process.exe", true, true)]
        [TestCase("+<pro*>[*]*", "process.exe", true, true)]
        [TestCase("+<*cess>[*]*", "process.exe", true, true)]
        [TestCase("+[ABC*]*", "nunit-executable.exe", true, true)]
        [TestCase("+[*]DEF.*", "nunit-executable.exe", true, true)]
        [TestCase("+[*]*", "process.exe", true, true)]
        [TestCase("-[ABC*]*", "nunit-executable.exe", true, true)]
        [TestCase("-[*]DEF.*", "nunit-executable.exe", true, true)]
        [TestCase("-[*]*", "process.exe", false, false)]
        [TestCase("-<*>[*]* +<pro*>[*]*", "process.exe", false, false)]
        [TestCase("+<abc*>[*]* +<pro*>[*]*", "process.exe", true, true)]
        [TestCase("-<*>[ABC*]* +[*]*", "process.exe", true, true)]
        [TestCase("-<*>[ABC*]* +<*>[*]*", "process.exe", true, true)]
        [TestCase("-<pro*>[D*F]* +[*]*", "process.exe", true, true)]
        [TestCase("-<*cess>[*GHI]* +[*]*", "process.exe", true, true)]
        [TestCase("+<ABC>[*]*", "process.exe", false, false)]
        [TestCase("+<pro*>[*]*", "process.exe", true, true)]
        #endregion

        #region match no drive-path-extension, only process-name (same as above)
        [TestCase("-<pro*>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase("+<pro*>[*]*", @"C:\Debug\process.exe", true, true)]
        [TestCase("-<*cess>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase("+<*cess>[*]*", @"C:\Debug\process.exe", true, true)]
        #endregion

        #region match full-path-process-name (path\name\ext)
        [TestCase(@"-<C:\Debug\pro*>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase(@"+<C:\Debug\pro*>[*]*", @"C:\Debug\process.exe", true, true)]

        [TestCase(@"-<*cess.exe>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase(@"-<*cess.dll>[*]*", @"C:\Debug\process.exe", true, true)]

        [TestCase(@"+<*cess.dll>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase(@"+<*cess.exe>[*]*", @"C:\Debug\process.exe", true, true)]
        #endregion

        #region match when both filters, when no exclude filters, or when no include filters or when no filters at all
        // 1/1 match include filter if not excluded
        [TestCase(@"-<C:\Debug\pro*>[*]* +<noprocess>[*]*", @"C:\Release\process.exe", false, false)]
        [TestCase(@"-<C:\Debug\pro*>[*]* +<process>[*]*", @"C:\Release\process.exe", true, true)]

        // 1/0 include if not excluded and no include filters
        [TestCase(@"-<C:\Debug\pro*>[*]*", @"C:\Release\process.exe", true, true)]

        // 0/1 match include filter if no exclude filters exists
        [TestCase(@"+<C:\Debug\pro*>[*]*", @"C:\Release\process.exe", false, false)]
        [TestCase(@"+<C:\Debug\pro*>[*]*", @"C:\Debug\process.exe", true, true)]
        
        // 0/0 always include if no exclude and no include filters
        [TestCase(@"", @"C:\Release\process.exe", true, true)]
        #endregion

        #region exclude only when filter does not ends with [*]*
        [TestCase(@"-<*>[*]*", @"C:\Debug\process.exe", false, false)]
        [TestCase(@"-<*>[*x*]*", @"C:\Debug\process.exe", true, true)]
        [TestCase(@"-<*>[*]*x*", @"C:\Debug\process.exe", true, true)]
        [TestCase(@"-<*>[*x*]*x*", @"C:\Debug\process.exe", true, true)]
        #endregion

        #region always include matching process regardless how process filter ends ([*]*|[*x*]*x*)
        [TestCase(@"+<*>[*]*", @"C:\Debug\process.exe", true, true)]
        [TestCase(@"+<*>[*x*]*", @"C:\Debug\process.exe", true, true)]
        [TestCase(@"+<*>[*]*x*", @"C:\Debug\process.exe", true, true)]
        [TestCase(@"+<*>[*x*]*x*", @"C:\Debug\process.exe", true, true)]
        #endregion

        #region never exclude proces that matches default-assembly-exclusion-filters (ie "mscorlib" when exclusion filters enabled)
        [TestCase(@"-<C:\Debug\pro*>[*]*", @"C:\dotNet\mscorlib.dll", true, true)]

        // issue found by user #329
        [TestCase(@"+[Open*]* -[OpenCover.T*]* -[*nunit*]*", @"C:\Release\nunit-console.exe.exe", true, true)]

        #endregion

        #region Cover last branches with invalid path chars (Path.GetInvalidPathChars)
        [TestCase(@"+<*>[*]*", "C:\\Debug\\process.exe|<>\"", true, true)]
        #endregion

        public void CanFilterByProcessName(string filterArg, string processName, bool expectedNoDefaultFilters, bool expectedWithDefaultFilters)
        {
            // arrange without default filters
            var filter = Filter.BuildFilter(new CommandLineParser(GetFilter(filterArg, false).ToArray()).Do(_ => _.ExtractAndValidateArguments()));

            // act
            var instrument = filter.InstrumentProcess(processName);

            // assert
            Assert.AreEqual(expectedNoDefaultFilters, instrument);

            // arrange again with default filters
            filter = Filter.BuildFilter(new CommandLineParser(GetFilter(filterArg, true).ToArray()).Do(_ => _.ExtractAndValidateArguments()));

            // act
            instrument = filter.InstrumentProcess(processName);

            // assert
            Assert.AreEqual(expectedWithDefaultFilters, instrument);
        }

        static IEnumerable<string> GetFilter(string filterArg, bool defaultFilters)
        {
            yield return "-target:t";
            yield return string.Format("-filter:\"{0}\"", filterArg);
            if (!defaultFilters) yield return "-nodefaultfilters";
        }
    }
}
