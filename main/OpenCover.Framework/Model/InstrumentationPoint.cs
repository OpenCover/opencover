using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An instrumentable point
    /// </summary>
    public class InstrumentationPoint
    {
        private static int _instrumentPoint;
        private static readonly List<int> _instrumentPoints;

        static InstrumentationPoint()
        {
            _instrumentPoints = new List<int>(8000) {0};
        }

        /// <summary>
        /// Get the number of recorded visit points for this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        static public int GetCount(uint spid)
        {
            return _instrumentPoints[(int)spid];
        }

        /// <summary>
        /// Add a number of recorded visit pints against this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        /// <param name="sum">the number of visit points to add</param>
        static public void AddCount(uint spid, int sum = 1)
        {
            _instrumentPoints[(int)spid] += sum;
        }

        public InstrumentationPoint()
        {
            UniqueSequencePoint = (UInt32)Interlocked.Increment(ref _instrumentPoint);
            _instrumentPoints.Add(0);
        }

        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

        [XmlAttribute("uspid")]
        public UInt32 UniqueSequencePoint { get; set; }

        [XmlAttribute("ordinal")]
        public UInt32 Ordinal { get; set; }

        [XmlAttribute("offset")]
        public int Offset { get; set; }
    }
}