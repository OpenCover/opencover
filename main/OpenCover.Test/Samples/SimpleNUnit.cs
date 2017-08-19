using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        [TestCaseSource("DivideCases")]
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
        }

        static object[] DivideCases = {
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
