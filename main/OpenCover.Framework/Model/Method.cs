//
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
            if (MethodPoint != null) MethodPoint.IsSkipped = true;
            MethodPoint = null;
            SequencePoints = null;
            BranchPoints = null;
        }

        #region IsGenerated & CallName  

        /// <summary>
        /// True if this.FullName matches generated-method-regex-pattern 
        /// </summary>
        /// <remarks>ATTN: Do not use as .Where predicate</remarks>
        internal bool IsGenerated {
            get {
        		if (_resolvedIsGenerated == null) {
        			_resolvedIsGenerated = !String.IsNullOrWhiteSpace(this.FullName)
                        && this.FullName.Contains("__") // quick test before using regex heavy weapon
                        && isGeneratedMethodRegex.IsMatch(this.FullName); 
        		}
        		return _resolvedIsGenerated == true;
            }
        }

        /// <summary>
        /// Method "::CallName(". (Name excluding return type, namespace and arguments)
        /// </summary>
        /// <remarks>ATTN: Do not use as .Where predicate</remarks>
        internal string CallName {
            get {
                if (_resolvedCallName != null) { return _resolvedCallName; } // cached
                _resolvedCallName = String.Empty; // init resolve value
                if (!String.IsNullOrWhiteSpace(this.FullName)) {
                    int startIndex = this.FullName.IndexOf("::", StringComparison.Ordinal);
                    startIndex += 2;
                    int finalIndex = this.FullName.IndexOf('(', startIndex);
                    if (startIndex > 1 && finalIndex > startIndex) {
                        _resolvedCallName = this.FullName // resolve cache
                            .Substring(startIndex, finalIndex - startIndex);
                    }
                }
                return _resolvedCallName;
            }
        }

        private bool? _resolvedIsGenerated = null;
        private string _resolvedCallName = null;
        private static readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
        private static readonly Regex isGeneratedMethodRegex = new Regex(@"(<[^\s:>]+>\w__\w)", regexOptions);

        #endregion

        #region Extracting structure from FullName using Regex

        /// <summary>
        /// returnType in Regex(returnType namespacePrefix::methodName([argumentTypes])
        /// </summary>
        internal string RegexReturnType {
            get {
                extractSignature();
                return _returnType;
            }
        }

        /// <summary>
        /// nameSpacePrefix in Regex(returnType namespacePrefix::methodName([argumentTypes])
        /// </summary>
        internal string RegexNameSpaceAndClass {
            get {
                extractSignature();
                return _nameSpaceAndClass;
            }
        }

        /// <summary>
        /// methodName in Regex(returnType namespacePrefix::methodName([argumentTypes])
        /// </summary>
        internal string RegexCallName {
            get {
                extractSignature();
                return _callName;
            }
        }

        /// <summary>
        /// optional argumentTypes in Regex(returnType namespacePrefix::methodName([argumentTypes])
        /// </summary>
        internal string RegexArgumentTypes {
            get {
                extractSignature();
                return _argumentTypes;
            }
        }

        /// <summary>
        /// RegexReplacedName != String.Empty
        /// </summary>
        internal bool RegexIsGenerated {
        	get {
                extractSignature();
                return _originalCallName != String.Empty;
        	}
        }

        /// <summary>
        /// Original methodName that is replaced by new generated methodName
        /// </summary>
        internal string RegexOriginalCallName {
            get {
                extractSignature();
                return _originalCallName;
            }
        }

        private void extractSignature () {

            if (!_methodSignatureExtracted) { // not yet extracted?
                _methodSignatureExtracted = true;

                if (string.IsNullOrWhiteSpace(this.FullName)) {
                    return; // method name is null or empty or white-space
                }

                var match = methodItemsRegex.Match(this.FullName);
                if (match.Success) {
                    foreach (var groupName in methodItemsRegex.GetGroupNames()) {
                        switch (groupName) {
                            case "returnType":
                                _returnType = match.Groups[groupName].Value;
                                break;
                            case "nameSpaceAndClass":
                                _nameSpaceAndClass = match.Groups[groupName].Value;
                                break;
                            case "callName":
                                _callName = match.Groups[groupName].Value;
                                break;
                            case "argumentTypes":
                                _argumentTypes = match.Groups[groupName].Value;
                                break;
                        }
                    }

                    var subMatch = replacedNameRegex.Match(this.FullName);
                    if (subMatch.Success) {
                        _originalCallName = subMatch.Groups["originalCallName"].Value;
                    }
                }
            }
        }

        private static readonly Regex methodItemsRegex = new Regex(@"^(?<returnType>[^\s]+)\s(?<nameSpaceAndClass>[^\s:]+)::(?<callName>[^\(]+)\((?<argumentTypes>[^\)]+)?\)$", regexOptions);
        private static readonly Regex replacedNameRegex = new Regex(@"<(?<originalCallName>[^\s:>]+)>\w__\w", regexOptions);

        private bool _methodSignatureExtracted = false;
        private string _returnType = String.Empty;
        private string _nameSpaceAndClass = String.Empty;
        private string _callName = String.Empty;
        private string _argumentTypes = String.Empty;
        private string _originalCallName = String.Empty;

        #endregion
    }
}
