using System.Linq;
using NUnit.Framework;
using OpenCover.Extensions.Strategy;

namespace OpenCover.Test.Extensions.Strategy
{
    [TestFixture]
    public class TrackNUnitTestMethodsTests
    {
        private TrackNUnitTestMethods _strategy;
        private Mono.Cecil.AssemblyDefinition _assemblyDefinition;

        [SetUp]
        public void SetUp()
        {
            _strategy = new TrackNUnitTestMethods();
            _assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(typeof(TrackNUnitTestMethodsTests).Assembly.Location);
        }

        [Test]
        public void Can_Identify_Methods()
        {
            // arrange            
            
            // act
            var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::ASingleTest()")));
        }

        [Test]
        public void TestAttribute_Is_Recognized()
        {
            // arrange            

            // act
            var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::ASingleTestCase()")));
        }


        [Test]
        public void TheoryAttribute_Is_Recognized()
        {
            // arrange            

            // act
            var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::TheoryTest(System.Double)")));
        }

        [Test]
        public void TestCaseSourceAttribute_Is_Recognized()
        {
          // arrange            

          // act
          var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

          // assert
          Assert.True(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::DivideTest(System.Int32,System.Int32,System.Int32)")));
        }

        [Test]
        public void Repeat_Is_Not_Recognized()
        {
            // arrange            

            // act
            var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

            // assert
            Assert.False(methods.Any(x => x.FullName.EndsWith("SimpleNUnit::RepeatWithoutTest()")));
        }

        [Test]
        public void Can_Identify_Methods_InNestedClasses()
        {
            // arrange            

            // act
            var methods = _strategy.GetTrackedMethods(_assemblyDefinition.MainModule.Types);

            // assert
            Assert.True(methods.Any(x => x.FullName.EndsWith(".Samples.ComplexNUnit/InnerTests::InnerExecuteMethod()")));
        }
    }
}