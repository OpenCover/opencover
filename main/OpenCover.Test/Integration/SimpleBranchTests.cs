using NUnit.Framework;

namespace OpenCover.Test.Integration
{
    [TestFixture(Description = "Theses tests are targets used for integration testing")]
    [Category("Integration")]
    [Explicit("Integration")]
    public class SimpleBranchTests
    {
        [Test]
        public void SimpleIf()
        {
            bool x = true;
            if (x)
            {
                System.Diagnostics.Debug.WriteLine("X=SimpleIf(true)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("X=SimpleIf(false)");
            }

            bool y = true;
            if (!y)
            {
                System.Diagnostics.Debug.WriteLine("Y=SimpleIf(true)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Y=SimpleIf(false)");
            }
        }
    }
}