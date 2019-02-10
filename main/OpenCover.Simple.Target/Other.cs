using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Namespace
{
    public class Other
    {
        public Other()
        {
        }

        public int Data { get; set; }

#pragma warning disable 649 // deliberately not used
        private int _x;

        public int X { get { return _x; } }
    }
}
