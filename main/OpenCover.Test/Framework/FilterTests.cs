using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class FilterTests
    {
        #region TestData

        public class AssemblyClassData
        {
            public string AssemblyClass { get; set; }
            public string AssemblyResult { get; set; }
            public string ClassResult { get; set; }
            public FilterType FilterTypeResult { get; set; }
        }

#pragma warning disable 169

        private string[] _invalidAssemblyClassPairs = new[] { "Garbage", "+[]", "-[ ]", "[ ", " ]", "+[]]", "-[][", @"-[\]", @"+[X]\", "-[X]]", "+[X][" };

        private AssemblyClassData[] _assemblyClassPairs = new[]
                                                              {
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]Console",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "-[System.*]Console",
                                                                          AssemblyResult = @"System\..*",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Exclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]Console.*",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = @"Console\..*",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
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
                filter.InclusionFilter.Count : filter.ExclusionFilter.Count);

            Assert.AreEqual(assemblyClassPair.AssemblyResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilter[0].Key : filter.ExclusionFilter[0].Key);

            Assert.AreEqual(assemblyClassPair.ClassResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilter[0].Value : filter.ExclusionFilter[0].Value);
        }

        [Test]
        public void UseAssembly_ReturnsFalse_WhenAssembly_Matches_NoFilters()
        {
            // arrange
            var filter = new Filter();
            
            // act
            var result = filter.UseAssembly("System.Debug");

            // result
            Assert.IsFalse(result);
        }

        [Test]
        public void UseAssembly_ReturnsFalse_WhenAssembly_Matches_ExclusionFilter()
        {
            // arrange
            var filter = new Filter();
            filter.AddFilter("-[System.*]*");

            // act
            var result = filter.UseAssembly("System.Debug");

            // result
            Assert.IsFalse(result);
        }

        [Test]
        public void UseAssembly_ReturnsTrue_WhenAssembly_Matches_InclusionFilter()
        {
            // arrange
            var filter = new Filter();
            filter.AddFilter("+[System.*]*");

            // act
            var result = filter.UseAssembly("System.Debug");

            // result
            Assert.IsTrue(result);
        }

        [Test]
        public void UseAssembly_ReturnsFalse_WhenAssembly_Matches_ExclusionFilterFirst()
        {
            // arrange
            var filter = new Filter();
            filter.AddFilter("-[System.*]*");
            filter.AddFilter("+[System.*]*");

            // act
            var result = filter.UseAssembly("System.Debug");

            // result
            Assert.IsFalse(result);
        }
    }
}
