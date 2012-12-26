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
    public class Method : SkippedEntity
    {
        /// <summary>
        /// The MetadataToken used to identify this entity within the assembly
        /// </summary>
        public int MetadataToken { get; set; }

        /// <summary>
        /// The name of the method including namespace, return type and arguments
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A reference to a file in the file collection (used to help visualisation)
        /// </summary>
        public FileRef FileRef { get; set; }

        /// <summary>
        /// A list of sequence points that have been produced for this method
        /// </summary>
        public SequencePoint[] SequencePoints { get; set; }

        /// <summary>
        /// A list of branch points that have been identified for this method
        /// </summary>
        public BranchPoint[] BranchPoints { get; set; }

        /// <summary>
        /// A method point to identify the entry of a method
        /// </summary>
        public InstrumentationPoint MethodPoint { get; set; }

        /// <summary>
        /// Has the method been visited
        /// </summary>
        [XmlAttribute("visited")]
        public bool Visited { get; set; }

        /// <summary>
        /// What is the cyclomatic complexity of this method.
        /// </summary>
        /// <remarks>Calculated using the Gendarme rules library</remarks>
        [XmlAttribute("cyclomaticComplexity")]
        public int CyclomaticComplexity { get; set; }

        /// <summary>
        /// What is the sequence coverage of this method
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("sequenceCoverage")]
        public int SequenceCoverage { get; set; }

        /// <summary>
        /// What is the branch coverage of this method
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("branchCoverage")]
        public int BranchCoverage { get; set; }

        /// <summary>
        /// Is this method a constructor
        /// </summary>
        [XmlAttribute("isConstructor")]
        public bool IsConstructor { get; set; }

        /// <summary>
        /// Is this method static
        /// </summary>
        [XmlAttribute("isStatic")]
        public bool IsStatic { get; set; }

        /// <summary>
        /// Is this method a property getter
        /// </summary>
        [XmlAttribute("isGetter")]
        public bool IsGetter { get; set; }
        
        /// <summary>
        /// Is this method a property setter
        /// </summary>
        [XmlAttribute("isSetter")]
        public bool IsSetter { get; set; }

        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
            if (MethodPoint != null) MethodPoint.IsSkipped = true;
            MethodPoint = null;
            SequencePoints = null;
            BranchPoints = null;
        }
    }
}
