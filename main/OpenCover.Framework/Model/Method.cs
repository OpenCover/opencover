﻿//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An method entity that can be instrumented
    /// </summary>
    public class Method : SummarySkippedEntity
    {

        /// <summary>
        /// The MetadataToken used to identify this entity within the assembly
        /// </summary>
        public int MetadataToken { get; set; }

        /// <summary>
        /// The full name of the method (method-definition), includes return-type namespace-class::call-name(argument-types)
        /// </summary>
        [XmlElement("Name")]
        public string FullName { get; set; }

        /// <summary>
        /// A reference to a file in the file collection (used to help visualisation)
        /// </summary>
        public FileRef FileRef { get; set; }

        internal UInt32 FileRefUniqueId {
            get { return FileRef == null? 0 : FileRef.UniqueId; }
        }

        /// <summary>
        /// A list of sequence points that have been produced for this method
        /// </summary>
        public SequencePoint[] SequencePoints {
            get {
                return _sequencePoints;
            }
            set {
                _sequencePoints = value ?? new SequencePoint[0];
            }
        }
        private SequencePoint[] _sequencePoints = new SequencePoint[0];

        /// <summary>
        /// A list of branch points that have been identified for this method
        /// </summary>
        public BranchPoint[] BranchPoints {
            get {
                return _branchPoints;
            }
            set {
                _branchPoints = value ?? new BranchPoint[0];
            }
        }
        private BranchPoint[] _branchPoints = new BranchPoint[0];

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
        /// What is the NPath complexity of this method.
        /// </summary>
        /// <remarks>Product of path branches (ie:path0=2;path1=3;path2=2 =&gt;2*3*2==12</remarks>
        [XmlAttribute("nPathComplexity")]
        public int NPathComplexity { get; set; }

        /// <summary>
        /// What is the sequence coverage of this method
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("sequenceCoverage")]
        public decimal SequenceCoverage { get; set; }

        /// <summary>
        /// What is the branch coverage of this method
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("branchCoverage")]
        public decimal BranchCoverage { get; set; }

        /// <summary>
        /// What is the crap score of this method
        /// based on the following calculation
        /// CRAP1(m) = comp(m)^2 * (1 – cov(m)/100)^3 + comp(m)
        /// </summary>
        /// <remarks>Rounded for ease</remarks>
        [XmlAttribute("crapScore")]
        public decimal CrapScore { get; set; }

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

        /// <summary>
        /// Mark an entity as skipped
        /// </summary>
        /// <param name="reason">Provide a reason</param>
        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
            if (MethodPoint != null) 
                MethodPoint.IsSkipped = true;
            MethodPoint = null;
            SequencePoints = null;
            BranchPoints = null;
        }

        #region IsGenerated & CallName  

        /// <summary>
        /// True if this.FullName matches generated-method-regex-pattern 
        /// </summary>
        internal bool IsGenerated {
            get {
        		if (_resolvedIsGenerated == null) {
        			_resolvedIsGenerated = !string.IsNullOrWhiteSpace(FullName)
                        && FullName.Contains("__") // quick test before using regex heavy weapon
                        && IsGeneratedMethodRegex.IsMatch(FullName); 
        		}
        		return _resolvedIsGenerated == true;
            }
        }

        /// <summary>
        /// Method "::CallName(". (Name excluding return type, namespace and arguments)
        /// </summary>
        internal string CallName {
            get {
                if (_resolvedCallName != null)
                    return _resolvedCallName;
                _resolvedCallName = string.Empty; // init resolve value
                if (!string.IsNullOrWhiteSpace(FullName)) {
                    var startIndex = FullName.IndexOf("::", StringComparison.Ordinal);
                    startIndex += 2;
                    var finalIndex = FullName.IndexOf('(', startIndex);
                    if (startIndex > 1 && finalIndex > startIndex) {
                        _resolvedCallName = FullName // resolve cache
                            .Substring(startIndex, finalIndex - startIndex);
                    }
                }
                return _resolvedCallName;
            }
        }

        private bool? _resolvedIsGenerated;
        private string _resolvedCallName;
        private const RegexOptions RegexOptions = System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.ExplicitCapture;
        private static readonly Regex IsGeneratedMethodRegex = new Regex(@"(<[^\s:>]+>\w__\w)", RegexOptions);

        #endregion

    }
}
