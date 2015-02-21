using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenCover.Support.UITesting;

namespace OpenCover.Test.Support
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class UITestingHelperTests
    {
        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(UITestingHelperTests))]
        public void PropagateRequiredEnvironmentVariables_IgnoresUnexpectedTypes(Type type)
        {
            // arrange
            var instance = Activator.CreateInstance(type); // type must have a paramterless constructor

            // act/assert
            Assert.DoesNotThrow(() => UITestingHelper.PropagateRequiredEnvironmentVariables(instance));
        }

        [Test]
        [TestCase("COR_PROFILER_EX")]
        [TestCase("CHAIN_EXTERNAL_PROFILER_EX")]
        [TestCase("OPENCOVER_PROFILER_KEY_EX")]
        public void PropagateRequiredEnvironmentVariables_AddsMissingEnvironmentVariables(string env)
        {
            // arrange
            var pi = new ProcessStartInfo();
            Assert.IsFalse(pi.EnvironmentVariables.ContainsKey(env));

            Environment.SetEnvironmentVariable(env, "random");

            // act
            UITestingHelper.PropagateRequiredEnvironmentVariables(pi);

            // assert
            Assert.AreEqual("random", pi.EnvironmentVariables[env]);
        }

        [Test]
        [TestCase("COR_PROFILER_EX")]
        [TestCase("CHAIN_EXTERNAL_PROFILER_EX")]
        [TestCase("OPENCOVER_PROFILER_KEY_EX")]
        public void PropagateRequiredEnvironmentVariables_DoesNotAlterCurrentEnvironmentVariables(string env)
        {
            // arrange
            var pi = new ProcessStartInfo();
            Environment.SetEnvironmentVariable(env, "random");
            Assert.AreEqual("random", pi.EnvironmentVariables[env]);

            Environment.SetEnvironmentVariable(env, "new random");

            // act
            UITestingHelper.PropagateRequiredEnvironmentVariables(pi);

            // assert
            Assert.AreEqual("random", pi.EnvironmentVariables[env]);
        }

        [Test]
        [TestCase("WIBBLE")]
        [TestCase("OOPSY")]
        [TestCase("STUFF")]
        public void PropagateRequiredEnvironmentVariables_DoesNotAddOtherEnvironmentVariables(string env)
        {
            // arrange
            var pi = new ProcessStartInfo();
            Assert.IsFalse(pi.EnvironmentVariables.ContainsKey(env));

            Environment.SetEnvironmentVariable(env, "random");

            // act
            UITestingHelper.PropagateRequiredEnvironmentVariables(pi);

            // assert
            Assert.IsFalse(pi.EnvironmentVariables.ContainsKey(env));
        }
    }
}
