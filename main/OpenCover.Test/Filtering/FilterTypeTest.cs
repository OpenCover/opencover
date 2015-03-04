using System;
using NUnit.Framework;
using OpenCover.Framework.Filtering;

namespace OpenCover.Test.Filtering
{
    [TestFixture]
    public class FilterTypeTest
    {
        [Test]
        public void ParseFilterType_Plus_Is_Inclusion()
        {
            Assert.That("+".ParseFilterType(), Is.EqualTo(FilterType.Inclusion));
        }

        [Test]
        public void ParseFilterType_Minus_Is_Exclusion()
        {
            Assert.That("-".ParseFilterType(), Is.EqualTo(FilterType.Exclusion));
        }

        [Test]
        public void ParseFilterType_Unknown_Throws()
        {
            Assert.Throws(typeof(ArgumentException), () => "/".ParseFilterType());
        }
    }
}
