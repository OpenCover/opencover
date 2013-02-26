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
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(new[] { null, "" });

            Assert.Equal(1, filter.ExcludedAttributes.Count);
        }
    }
}
