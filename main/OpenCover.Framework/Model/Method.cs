//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that can be instrumented
    /// </summary>
    public class Method
    {
        public int MetadataToken { get; set; }
        public string Name { get; set; }
        public FileRef FileRef { get; set; }
        public SequencePoint[] SequencePoints { get; set; }

        [XmlAttribute("cyclomaticComplexity")]
        public int CyclomaticComplexity { get; set; }
    }
}
