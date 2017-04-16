using System.Linq;
using NUnit.Framework;
using OpenCover.Extensions.Strategy;
using OpenCover.Framework.Strategy;

namespace OpenCover.Test.Extensions.Strategy
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
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleXUnit::AddAttributeExclusionFilters_Handles_Null_Elements()")));
        }

        [Test]
        public void Can_Identify_Theories()
        {
            // arrange
            var strategy = new TrackXUnitTestMethods();

            var def = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof(TrackXUnitTestMethodsTests).Assembly.Location);

            // act
            var methods = strategy.GetTrackedMethods(def.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleXUnit::Test_Data_Is_Even(System.Int32)")));
        }
    }
}