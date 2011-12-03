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
        public BranchPoint[] BranchPoints { get; set; }

        public InstrumentationPoint MethodPoint { get; set; }

        [XmlAttribute("visited")]
        public bool Visited { get; set; }

        [XmlAttribute("cyclomaticComplexity")]
        public int CyclomaticComplexity { get; set; }

        [XmlAttribute("sequenceCoverage")]
        public int SequenceCoverage { get; set; }

        [XmlAttribute("branchCoverage")]
        public int BranchCoverage { get; set; }

        [XmlAttribute("isConstructor")]
        public bool IsConstructor { get; set; }

        [XmlAttribute("isStatic")]
        public bool IsStatic { get; set; }

        [XmlAttribute("isGetter")]
        public bool IsGetter { get; set; }
        
        [XmlAttribute("isSetter")]
        public bool IsSetter { get; set; }
    }
}
