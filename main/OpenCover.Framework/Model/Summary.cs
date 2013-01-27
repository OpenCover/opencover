using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A summary of results
    /// </summary>
    public class Summary
    {
        /// <summary>
        /// The number of sequence points
        /// </summary>
        [XmlAttribute("numSequencePoints")]
        public int NumSequencePoints { get; set; }

        /// <summary>
        /// The number of sequence points visited
        /// </summary>
        [XmlAttribute("visitedSequencePoints")]
        public int VisitedSequencePoints { get; set; }

        /// <summary>
        /// The number of branch points
        /// </summary>
        [XmlAttribute("numBranchPoints")]
        public int NumBranchPoints { get; set; }

        /// <summary>
        /// The number of branch points visited
        /// </summary>
        [XmlAttribute("visitedBranchPoints")]
        public int VisitedBranchPoints { get; set; }

        /// <summary>
        /// What is the sequence coverage?
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("sequenceCoverage")]
        public decimal SequenceCoverage { get; set; }

        /// <summary>
        /// What is the branch coverage?
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("branchCoverage")]
        public decimal BranchCoverage { get; set; }

        /// <summary>
        /// What is the maximum cyclomatic complexity.
        /// </summary>
        /// <remarks>Calculated using the Gendarme rules library</remarks>
        [XmlAttribute("maxCyclomaticComplexity")]
        public int MaxCyclomaticComplexity { get; set; }

        /// <summary>
        /// What is the minimum cyclomatic complexity.
        /// </summary>
        /// <remarks>Calculated using the Gendarme rules library</remarks>
        [XmlAttribute("minCyclomaticComplexity")]
        public int MinCyclomaticComplexity { get; set; }
    }
}
