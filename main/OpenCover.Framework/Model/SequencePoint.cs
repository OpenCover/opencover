//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// a sequence point
    /// </summary>
    public class SequencePoint : InstrumentationPoint, IDocumentReference, IEquatable<SequencePoint>
    {        
        /// <summary>
        /// The start line of the sequence point
        /// </summary>
        [XmlAttribute("sl")]
        public int StartLine { get; set; }

        /// <summary>
        /// The start column of the sequence point
        /// </summary>
        [XmlAttribute("sc")]
        public int StartColumn { get; set; }

        /// <summary>
        /// The end line of the sequence point
        /// </summary>
        [XmlAttribute("el")]
        public int EndLine { get; set; }

        /// <summary>
        /// The end column of the sequence point
        /// </summary>
        [XmlAttribute("ec")]
        public int EndColumn { get; set; }

        /// <summary>
        /// Count of merged branches
        /// </summary>
        /// <summary>
        /// The number of branch exits
        /// </summary>
        [XmlAttribute("bec")]
        public int BranchExitsCount { get; set; }
        
        /// <summary>
        /// Visit count of merged branches 
        /// </summary>
        [XmlAttribute("bev")]
        public int BranchExitsVisit { get; set; }

        /// <summary>
        /// The file associated with the supplied startline 
        /// </summary>
        [XmlAttribute("fileid")]
        public uint FileId { get; set; }

        /// <summary>
        /// The url to the document if an entry was not mapped to an id
        /// </summary>
        [XmlAttribute("url")]
        public string Document { get; set; }
        
        internal List<BranchPoint> BranchPoints { get; set; }

        /// <summary>
        /// Property
        /// </summary>
        public bool IsSingleCharSequencePoint {
        	get {
	            return (StartLine == EndLine) && (EndColumn - StartColumn) == 1;
        	}
        }

		#region IEquatable implementation

		/// <summary>
		/// Override GetHashCode
		/// </summary>
		/// <returns>int</returns>
        public override int GetHashCode () {
			return unchecked (StartLine << 3) ^ unchecked (EndLine << 2) ^ unchecked (StartColumn << 1) ^ (EndColumn);
        }
		
		/// <summary>
		/// Override Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>bool</returns>
        public override bool Equals (object obj) {
            var that = obj as SequencePoint;
            return !ReferenceEquals(that, null) && (ReferenceEquals(this, that) || (this as IEquatable<SequencePoint>).Equals(that));
        }

		/// <summary>
		/// IEquatable&lt;SequencePoint&gt;.Equals implementation
		/// </summary>
		/// <param name="other">SequencePoint</param>
		/// <returns>bool</returns>
		bool IEquatable<SequencePoint>.Equals(SequencePoint other)
		{
			return !ReferenceEquals(other, null)
				&& FileId != 0
				&& FileId == other.FileId
				&& StartLine == other.StartLine
				&& StartColumn == other.StartColumn
				&& EndLine == other.EndLine
				&& EndColumn == other.EndColumn;
		}

		#endregion
    }
}
