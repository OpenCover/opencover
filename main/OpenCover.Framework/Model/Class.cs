using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    public class Class
    {
        public string FullName { get; set; }
        public IList<Method> Methods { get; set; }
    }
}
