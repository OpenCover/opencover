using System.Linq;
using NUnit.Framework;
using OpenCover.Extensions.Strategy;

namespace OpenCover.Test.Extensions.Strategy
{
    [TestFixture]
    public class TrackNUnitTestMethodsTests
    {
        private TrackNUnitTestMethods strategy;
        private Mono.Cecil.AssemblyDefinition assemblyDefinition;

        [SetUp]
        public void SetUp()
        {
            strategy = new TrackNUnitTestMethods();
            assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof(TrackNUnitTestMethodsTests).Assembly.Location);
        }

        [Test]
        public void Can_Identify_Methods()
        {
            // arrange            
            
            // act
            var methods = strategy.GetTrackedMethods(assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::ASingleTest()")));
        }

        [Test]
        public void TestAttribute_Is_Recognized()
        {
            // arrange            

            // act
            var methods = strategy.GetTrackedMethods(assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::ASingleTestCase()")));
        }


        [Test]
        public void TheoryAttribute_Is_Recognized()
        {
            // arrange            

            // act
            var methods = strategy.GetTrackedMethods(assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::TheoryTest(System.Double)")));
        }


        [Test]
        public void Repeat_Is_Not_Recognized()
        {
            // arrange            

            // act
            var methods = strategy.GetTrackedMethods(assemblyDefinition.MainModule.Types);

            // assert
            Assert.False(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::RepeatWithoutTest()")));
        }
    }
}