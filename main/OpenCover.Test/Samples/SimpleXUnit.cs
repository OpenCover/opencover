using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCover.Framework;
using OpenCover.Test.Framework.Persistance;
using Xunit;

namespace OpenCover.Test.Samples
{
    public class SimpleXUnit
    {
        [Fact]
        public void AddAttributeExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter(false);

            filter.AddAttributeExclusionFilters(new[] { null, "" });

            Assert.Equal(1, filter.ExcludedAttributes.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(8)]
        public void Test_Data_Is_Even(int i)
        {
            Assert.Equal(i % 2, 0);
        }
    }
}
