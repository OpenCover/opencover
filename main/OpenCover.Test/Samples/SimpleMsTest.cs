using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.QualityTools.Testing.Fakes;
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

    public class MyTime
    {
        public int GetTheCurrentYear()
        {
            return DateTime.Now.Year;
        }
    }

    [TestClass]
    public class FakesMsTest
    {
        [TestMethod]
        public void GetTheCurrentYear_When_2016()
        {
            // Shims can be used only in a ShimsContext:
            using (ShimsContext.Create())
            {
                // Arrange:
                // Shim DateTime.Now to return a fixed date:
                System.Fakes.ShimDateTime.NowGet =
                  () => new DateTime(2016, 1, 1);

                Assert.AreEqual(2016, (new MyTime()).GetTheCurrentYear());
            }
        }
    }
}
