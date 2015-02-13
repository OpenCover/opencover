using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using OpenCover.Support.Fakes;
using OpenCover.Support.UITesting;

namespace OpenCover.Test.Support
{
    [TestFixture]
    public class FakesHelperTests
    {
        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(StringDictionary))]
        [TestCase(typeof(FakesHelperTests))]
        public void LoadOpenCoverProfilerInstead_IgnoresUnexpectedTypes(Type type)
        {
            // arrange
            var instance = Activator.CreateInstance(type); // type must have a paramterless constructor

            // act/assert
            Assert.DoesNotThrow(() => FakesHelper.LoadOpenCoverProfilerInstead(instance));
        }

        [Test]
        public void LoadOpenCoverProfilerInstead_SwapsOutOriginalProfilerForOpenCover()
        {
            // arrange
            var dict = new Dictionary<string, string>()
            {
                {"COR_ENABLE_PROFILING", "1"},
                {"COR_PROFILER", "{0000002F-0000-0000-C000-000000000046}"} // not a real profiler just a COM object that should exist on every box
            };

            // act
            Assert.DoesNotThrow(() => FakesHelper.LoadOpenCoverProfilerInstead(dict));

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}", dict["COR_PROFILER"]);
            Assert.AreEqual("{0000002F-0000-0000-C000-000000000046}", dict["CHAIN_EXTERNAL_PROFILER"]);
            Assert.IsTrue(dict["CHAIN_EXTERNAL_PROFILER_LOCATION"].EndsWith("oleaut32.dll", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void LoadOpenCoverProfilerInstead_DoesNotAlterProfilerIfNotEnabled()
        {
            // arrange
            var dict = new Dictionary<string, string>()
            {
                {"COR_ENABLE_PROFILING", "0"},
                {"COR_PROFILER", "{0000002F-0000-0000-C000-000000000046}"} // not a real profiler just a COM object that should exist on every box
            };

            // act
            Assert.DoesNotThrow(() => FakesHelper.LoadOpenCoverProfilerInstead(dict));

            // assert
            Assert.AreEqual("{0000002F-0000-0000-C000-000000000046}", dict["COR_PROFILER"]);
        }

        [Test]
        public void LoadOpenCoverProfilerInstead_DoesNotAlterProfilerIfCurrentProfilerIsOpenCover()
        {
            // arrange
            var dict = new Dictionary<string, string>()
            {
                {"COR_ENABLE_PROFILING", "1"},
                {"COR_PROFILER", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}"} // not a real profiler just a COM object that should exist on every box
            };

            // act
            Assert.DoesNotThrow(() => FakesHelper.LoadOpenCoverProfilerInstead(dict));

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}", dict["COR_PROFILER"]);
            Assert.IsFalse(dict.ContainsKey("CHAIN_EXTERNAL_PROFILER"));
            Assert.IsFalse(dict.ContainsKey("CHAIN_EXTERNAL_PROFILER_LOCATION"));
        }

        [Test]
        public void PretendWeLoadedFakesProfiler_ChangesEnvironmentVariables()
        {
            // arrange
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");
            Environment.SetEnvironmentVariable("COR_PROFILER", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
            Environment.SetEnvironmentVariable("CHAIN_EXTERNAL_PROFILER", "{0000002F-0000-0000-C000-000000000046}");

            // act
            Assert.DoesNotThrow(() => FakesHelper.PretendWeLoadedFakesProfiler(null));

            // assert
            Assert.AreEqual("{0000002F-0000-0000-C000-000000000046}", Environment.GetEnvironmentVariable("COR_PROFILER"));
        }

        [Test]
        public void PretendWeLoadedFakesProfiler_DoesNotChangesEnvironmentVariablesIfNotEnables()
        {
            // arrange
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "0");
            Environment.SetEnvironmentVariable("COR_PROFILER", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
            Environment.SetEnvironmentVariable("CHAIN_EXTERNAL_PROFILER", "{0000002F-0000-0000-C000-000000000046}");

            // act
            Assert.DoesNotThrow(() => FakesHelper.PretendWeLoadedFakesProfiler(null));

            // assert
            Assert.AreEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}", Environment.GetEnvironmentVariable("COR_PROFILER"));
        }

        [Test]
        public void PretendWeLoadedFakesProfiler_DoesNotChangesEnvironmentVariablesIfNotOpenCover()
        {
            // arrange
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");
            Environment.SetEnvironmentVariable("COR_PROFILER", Guid.NewGuid().ToString());
            Environment.SetEnvironmentVariable("CHAIN_EXTERNAL_PROFILER", "{0000002F-0000-0000-C000-000000000046}");

            // act
            Assert.DoesNotThrow(() => FakesHelper.PretendWeLoadedFakesProfiler(null));

            // assert
            Assert.AreNotEqual("{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}", Environment.GetEnvironmentVariable("COR_PROFILER"));
        }
    }
}