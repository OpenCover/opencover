//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Linq;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// a branch point
    /// </summary>
    public class BranchPoint : InstrumentationPoint, IDocumentReference
    {
        /// <summary>
        /// Line of the branching instruction
        /// </summary>
        [XmlAttribute("sl")]
        public int StartLine { get; set; }

        /// <summary>
        /// A path that can be taken
        /// </summary>
        [XmlAttribute("path")]
        public int Path { get; set; }

        /// <summary>
        /// List of OffsetPoints between Offset and EndOffset (exclusive)
        /// </summary>
        [XmlAttribute("offsetchain")]
        public System.Collections.Generic.List<int> OffsetPoints { get; set; }
        
        /// <summary>
        /// Should offset points be serialized
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeOffsetPoints()
        {
            return OffsetPoints.Maybe(_ => _.Any());
        }

        /// <summary>
        /// Last Offset == EndOffset.
        /// Can be same as Offset
        /// </summary>
        [XmlAttribute("offsetend")]
        public int EndOffset { get; set; }

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
    }
}