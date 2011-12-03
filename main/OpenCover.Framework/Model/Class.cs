//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class
    {
        public string FullName { get; set; }
        [XmlIgnore]
        public File[] Files { get; set; }
        public Method[] Methods { get; set; }
        public bool Excluded { get; set; }
    }
}
