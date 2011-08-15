using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace OpenCover.Test.Samples
{
    public class Simple_xUnit
    {
        [Fact]
        public void MyTest()
        {
            Assert.Equal(4, 2 + 2);
        }
    }
}
