using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework.Strategy;
using OpenCover.Test.Samples;

namespace OpenCover.Test.Framework.Strategy
{
    [TestFixture]
    public class TrackMSTestTestMethodsTests
    {
        [Test]
        public void Can_Identify_Methods()
        {
            // arrange
            var strategy = new TrackMSTestTestMethods();

            var def = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof (TrackMSTestTestMethodsTests).Assembly.Location);

            // act
            var methods = strategy.GetTrackedMethods(def.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.Name.EndsWith("SimpleMsTest::BasePersistenceTests_All()")));
        }
    }
}
