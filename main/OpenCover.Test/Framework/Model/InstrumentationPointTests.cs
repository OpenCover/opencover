using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework.Model;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    public class InstrumentationPointTests
    {
        [Test]
        public void CanRetrieveSavedTrackedRefs()
        {
            var point = new InstrumentationPoint();

            point.TrackedMethodRefs = new []{new TrackedMethodRef(){UniqueId = 12345}};

            Assert.AreEqual(1, point.TrackedMethodRefs.Count());
            Assert.AreEqual(12345, point.TrackedMethodRefs[0].UniqueId);
        }


        [Test]
        public void CanClearSavedTrackedRefs()
        {
            var point = new InstrumentationPoint();

            point.TrackedMethodRefs = new[] { new TrackedMethodRef() { UniqueId = 12345 } };
            Assert.IsNotNull(point.TrackedMethodRefs);
            point.TrackedMethodRefs = null;
            Assert.IsNull(point.TrackedMethodRefs);
        }

    }
}
