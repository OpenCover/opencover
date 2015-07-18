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

        [Repeat(2)]        
        public void RepeatWithoutTest()
        {
            Console.WriteLine("Repeated test.");
        }
    }
}
