using NUnit.Framework;

namespace OpenCover.Test.Samples
{
    [TestFixture]
    public class ComplexNUnit
    {
        [TestFixture]
        public class InnerTests
        {
            [Test]
            public void InnerExecuteMethod()
            {
            }
        }
    }
}