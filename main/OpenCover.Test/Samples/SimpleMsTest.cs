using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCover.Test.Framework.Persistance;

namespace OpenCover.Test.Samples
{
    [TestClass]
    public class SimpleMsTest
    {
        [TestMethod]
        public void BasePersistenceTests_All()
        {
            var fixture = new BasePersistenceTests();
            var methods = typeof (BasePersistenceTests).GetMethods();
            foreach (var mi in methods)
            {
                if (mi.DeclaringType != typeof(BasePersistenceTests)) continue;
                if (mi.GetParameters().Any()) continue;
                fixture.SetUp();
                mi.Invoke(fixture, null);
                fixture.TearDown();
            }
        }
    }
}
