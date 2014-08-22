using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// a branch point
    /// </summary>
    public class BranchPoint : InstrumentationPoint
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
        /// Last Offset == EndOffset.
        /// Can be same as Offset
        /// </summary>
        [XmlAttribute("offsetend")]
        public int EndOffset { get; set; }
    }
}