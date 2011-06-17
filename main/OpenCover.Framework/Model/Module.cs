//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
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
        public File[] Files { get; set; }
        public Class[] Classes { get; set; }
    }
}
