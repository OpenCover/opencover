using NUnit.Framework;
using System;

namespace OpenCover.Test.Samples
{
    [TestFixture]
    public class SimpleNUnit
    {
        [Test]
        public void ASingleTest()
        {
        }

        [TestCase]
        public void ASingleTestCase()
        {
        }

        [Datapoint]
        public double parameterForTheory = 0;

        [Theory]
        public void TheoryTest(double parameter)
        {
        }

        [TestCaseSource(nameof(_divideCases))]
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
        }

        private static object[] _divideCases = {
            new object[] { 12, 3, 4 },
            new object[] { 12, 2, 6 },
            new object[] { 12, 4, 3 }
        };
    
        [Repeat(2)]        
        public void RepeatWithoutTest()
        {
            Console.WriteLine("Repeated test.");
        }
    }
}
