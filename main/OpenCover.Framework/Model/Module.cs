using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An assembly
    /// </summary>
    public class Module
    {
        public string FullName { get; set; }
        public IList<File> Files { get; set; }
        public IList<Class> Classes { get; set; }
    }
}
