using System.Linq;
using NUnit.Framework;
using OpenCover.Framework.Strategy;

namespace OpenCover.Test.Framework.Strategy
{
    [TestFixture]
    public class TrackXUnitTestMethodsTests
    {
        [Test]
        public void Can_Identify_Methods()
        {
            // arrange
            var strategy = new TrackXUnitTestMethods();

            var def = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof(TrackXUnitTestMethodsTests).Assembly.Location);

            // act
            var methods = strategy.GetTrackedMethods(def.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.Name.EndsWith("SimpleXUnit::AddAttributeExclusionFilters_Handles_Null_Elements()")));
        }
    }
}