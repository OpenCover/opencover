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
    public class SequencePoint : InstrumentationPoint, IDocumentReference
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
        
        internal List<BranchPoint> BranchPoints {
            get{
                return _branchPoints;
            }
            set{
                _branchPoints = value ?? new List<BranchPoint>();
            }
        }
        private List<BranchPoint> _branchPoints = new List<BranchPoint>();

        /// <summary>
        /// Property
        /// </summary>
        public bool IsSingleCharSequencePoint {
        	get {
	            return (StartLine == EndLine) && (EndColumn - StartColumn) == 1;
        	}
        }

        /// <summary>
        /// SonnarQube wants no more than 3 boolean conditions
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        private bool IsLineEqual (SequencePoint sp) {
            return StartLine == sp.StartLine && EndLine == sp.EndLine;
        }

        /// <summary>
        /// SonnarQube wants no more than 3 boolean conditions
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        private bool IsColumnEqual (SequencePoint sp) {
            return StartColumn == sp.StartColumn && EndColumn == sp.EndColumn;
        }

        /// <summary>
        /// Is Start/End Line/Column equal
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public bool IsPositionEqual (SequencePoint sp) {
            return sp != null && IsLineEqual (sp) && IsColumnEqual (sp);
        }

        /// <summary>
        /// Is FileId equal? (If FileId is 0 then file is unknown)
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public bool IsFileIdEqual (SequencePoint sp) {
            return sp != null && FileId != 0 && FileId == sp.FileId;
        }
    }
}
