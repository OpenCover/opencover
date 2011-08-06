//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
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
        private static readonly List<int> _sequencePoints;

        static SequencePoint()
        {
            _sequencePoints = new List<int>(8000) {0};
        }

        /// <summary>
        /// Get the number of recorded visit points for this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        static public int GetCount(uint spid)
        {
            return _sequencePoints[(int)spid];
        }

        /// <summary>
        /// Add a number of recorded visit pints against this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        /// <param name="sum">the number of visit points to add</param>
        static public void AddCount(uint spid, int sum = 1)
        {
            _sequencePoints[(int)spid] += sum;
        }

        public SequencePoint()
        {
            UniqueSequencePoint = (UInt32)Interlocked.Increment(ref _sequencePoint);
            _sequencePoints.Add(0);
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
