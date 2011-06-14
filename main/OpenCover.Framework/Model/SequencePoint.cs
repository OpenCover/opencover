using System;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An instrumentable point
    /// </summary>
    public class SequencePoint
    {
        private static int _sequencePoint;
        public SequencePoint()
        {
            UniqueSequencePoint = (UInt32)Interlocked.Increment(ref _sequencePoint);
        }

        [XmlAttribute("ordinal")]
        public UInt32 Ordinal { get; set; }

        [XmlAttribute("offset")]
        public int Offset { get; set; }
        
        [XmlAttribute("sl")]
        public int StartLine { get; set; }
        
        [XmlAttribute("sc")]
        public int StartColumn { get; set; }
        
        [XmlAttribute("el")]
        public int EndLine { get; set; }
        
        [XmlAttribute("ec")]
        public int EndColumn { get; set; }
        
        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

        [XmlAttribute("uspid")]
        public UInt32 UniqueSequencePoint { get; set; }
    }
}
