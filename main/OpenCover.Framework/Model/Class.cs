using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class
    {
        public string FullName { get; set; }
        public File[] Files { get; set; }
        public Method[] Methods { get; set; }
    }
}
