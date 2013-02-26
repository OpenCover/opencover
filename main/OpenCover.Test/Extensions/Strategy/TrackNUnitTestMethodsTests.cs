using System.Linq;
using NUnit.Framework;
using OpenCover.Framework.Strategy;

namespace OpenCover.Test.Extensions.Strategy
{
    [TestFixture]
    public class TrackNUnitTestMethodsTests
    {
        [Test]
        public void Can_Identify_Methods()
        {
            // arrange
            var strategy = new TrackNUnitTestMethods();

            var def = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof(TrackNUnitTestMethodsTests).Assembly.Location);

            // act
            var methods = strategy.GetTrackedMethods(def.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.Name.EndsWith("TrackNUnitTestMethodsTests::Can_Identify_Methods()")));
        }
    }
}