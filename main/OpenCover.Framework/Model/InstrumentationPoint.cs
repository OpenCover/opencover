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
        private static readonly List<InstrumentationPoint> _instrumentPoints;
        private static object _lock = new object();

        static InstrumentationPoint()
        {
            _instrumentPoints = new List<InstrumentationPoint>() {null};
        }

        /// <summary>
        /// Return the number of visit points
        /// </summary>
        public static int Count {
            get { return _instrumentPoints.Count; }
        }

        /// <summary>
        /// Get the number of recorded visit points for this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        public static int GetVisitCount(uint spid)
        {
            return _instrumentPoints[(int) spid].VisitCount;
        }

        /// <summary>
        /// Add a number of recorded visit pints against this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        /// <param name="trackedMethodId">the id of a tracked method - Note 0 means no method currently tracking</param>
        /// <param name="sum">the number of visit points to add</param>
        public static bool AddVisitCount(uint spid, uint trackedMethodId, int sum = 1)
        {
            if (spid != 0 && spid < _instrumentPoints.Count)
            {
                var point = _instrumentPoints[(int) spid];
                point.VisitCount += sum;
                if (trackedMethodId != 0)
                {
                    point._tracked = point._tracked ?? new List<TrackedMethodRef>();
                    var tracked = point._tracked.Find(x => x.UniqueId == trackedMethodId);
                    if (tracked == null)
                    {
                        tracked = new TrackedMethodRef() {UniqueId = trackedMethodId, VisitCount = sum};
                        point._tracked.Add(tracked);
                    }
                    else
                    {
                        tracked.VisitCount += sum;
                    }
                }
                return true;
            }
            return false;
        }

        private List<TrackedMethodRef> _tracked;

        public InstrumentationPoint()
        {
            lock (_lock)
            {
                UniqueSequencePoint = (UInt32) Interlocked.Increment(ref _instrumentPoint);
                _instrumentPoints.Add(this);
            }
        }

        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

        [XmlAttribute("uspid")]
        public UInt32 UniqueSequencePoint { get; set; }

        [XmlAttribute("ordinal")]
        public UInt32 Ordinal { get; set; }

        [XmlAttribute("offset")]
        public int Offset { get; set; }

        public TrackedMethodRef[] TrackedMethodRefs
        {
            get
            {
                return _tracked != null ? _tracked.ToArray() : null;
            }
            set
            {
                
            }
        }
    }
}
