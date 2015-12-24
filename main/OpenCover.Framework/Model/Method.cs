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

        /// <summary>
        /// method name excluding return type, namespace and arguments
        /// </summary>
        public string shortName {
        	get {
	        	if (String.IsNullOrWhiteSpace(this.Name)) return "";
				int startIndex = this.Name.IndexOf("::", StringComparison.Ordinal);
				int finalIndex = this.Name.IndexOf('(', startIndex);
				return this.Name
					.Substring(startIndex, finalIndex - startIndex)
					.Substring(2);
        	}
        }

        /// <summary>
        /// True if method name matches isGeneratedMethodRegex pattern
        /// </summary>
        public bool isGenerated {
        	get {
	        	return (!String.IsNullOrWhiteSpace(this.Name)
	        	        && this.Name.Contains("__")
	        	        && isGeneratedMethodRegex.IsMatch(this.Name)
	        	       );
        	}
        }

        private const RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
        private readonly Regex isGeneratedMethodRegex = new Regex(@"(<[^\s|>]+>[a-z]__\w(\w|_\w)?)(::([^\s|\(]+))?(\([^\s|\)]*\))$", regexOptions);
        private readonly Regex generatedMethodItems = new Regex(@"(?<returnType>[^\s]+)\s(?<nameSpace>[^\s|/]+/)?(?<className>[^\s|:]+::)?(<(?<replacedName>[^\s|>]+)>[a-z]__\w(\w|_\w)?)(::(?<methodName>[^\s|\(]+))?(\([^\s|\)]*\))$", regexOptions);

        /* Compiler Generated Name Examples
          <Name>System.Boolean DD.Collections.BitSetArray/&lt;Complement&gt;d__e::MoveNext()</Name>
          <Name>System.Boolean DD.Collections.BitSetArray::&lt;_SetItems&gt;b__b(System.Int32)</Name>
		  <Name>System.Boolean DD.Collections.BitSetArray::BitSetArray_&lt;_SetItems&gt;b__b_0(System.Int32)</Name>

		  <Name>[^\s]+\s[^\s|/|:]+(/\w*)?(::(.+_)?)?(&lt;\w+&gt;[a-z]__\w(\w|_\w)?)(::.+)?(\(.*\)</Name>)$
        */

        /*
            code sample
            Match match = generatedMethodItems.Match(sample);
            if (match.Success) Console.WriteLine(match.Groups["returnType"].Value);
        */
    }
}
