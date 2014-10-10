using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static readonly List<InstrumentationPoint> InstrumentPoints;

        static InstrumentationPoint()
        {
            _instrumentPoint = 0;
            InstrumentPoints = new List<InstrumentationPoint>(8192) {null};
        }

        static internal void Clear()
        {
            InstrumentPoints.Clear();
            InstrumentPoints.Add(null);
            _instrumentPoint = 0;
        }

        static internal void ResetAfterLoading()
        {
            var points = InstrumentPoints
                .Where(x => x != null)
                .GroupBy(x => x.UniqueSequencePoint)
                .Select(g => g.OrderBy(x => x.OrigSequencePoint).First())
                .ToList();

            var max = (int)points.Max(x => x.UniqueSequencePoint);
            
            InstrumentPoints.Clear();
            InstrumentPoints.Add(null);

            for (var i = 1; i <= max; i++)
            {
                var point = new SequencePoint();
                InstrumentPoints[i] = point;
                point.UniqueSequencePoint = (uint)i;
            }

            foreach (var instrumentationPoint in points)
            {
                InstrumentPoints[(int)instrumentationPoint.UniqueSequencePoint] = instrumentationPoint;
            }

            _instrumentPoint = max;
        }

        /// <summary>
        /// Return the number of visit points
        /// </summary>
        public static int Count {
            get { return InstrumentPoints.Count; }
        }

        /// <summary>
        /// Get the number of recorded visit points for this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        public static int GetVisitCount(uint spid)
        {
            return InstrumentPoints[(int) spid].VisitCount;
        }

        /// <summary>
        /// Add a number of recorded visit pints against this identifier
        /// </summary>
        /// <param name="spid">the sequence point identifier - NOTE 0 is not used</param>
        /// <param name="trackedMethodId">the id of a tracked method - Note 0 means no method currently tracking</param>
        /// <param name="sum">the number of visit points to add</param>
        public static bool AddVisitCount(uint spid, uint trackedMethodId, int sum = 1)
        {
            if (spid != 0 && spid < InstrumentPoints.Count)
            {
                var point = InstrumentPoints[(int) spid];
                point.VisitCount += sum;
                if (point.VisitCount < 0) 
                    point.VisitCount = int.MaxValue;
                if (trackedMethodId != 0)
                {
                    point._tracked = point._tracked ?? new List<TrackedMethodRef>();
                    var tracked = point._tracked.Find(x => x.UniqueId == trackedMethodId);
                    if (tracked == null)
                    {
                        tracked = new TrackedMethodRef {UniqueId = trackedMethodId, VisitCount = sum};
                        point._tracked.Add(tracked);
                    }
                    else
                    {
                        tracked.VisitCount += sum;
                        if (tracked.VisitCount < 0)
                            tracked.VisitCount = int.MaxValue;
                    }
                }
                return true;
            }
            return false;
        }

        private List<TrackedMethodRef> _tracked;

        /// <summary>
        /// Initialise
        /// </summary>
        public InstrumentationPoint()
        {
            UniqueSequencePoint = (uint)Interlocked.Increment(ref _instrumentPoint);
            InstrumentPoints.Add(this);
            OrigSequencePoint = UniqueSequencePoint;
        } 

        /// <summary>
        /// Store the number of visits
        /// </summary>
        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

        /// <summary>
        /// A unique number
        /// </summary>
        [XmlAttribute("uspid")]
        public UInt32 UniqueSequencePoint { get; set; }

        /// <summary>
        /// An order of the point within the method
        /// </summary>
        [XmlAttribute("ordinal")]
        public UInt32 Ordinal { get; set; }

        /// <summary>
        /// The IL offset of the point
        /// </summary>
        [XmlAttribute("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Used to hide an instrumentation point
        /// </summary>
        [XmlIgnore]
        public bool IsSkipped { get; set; }

        /// <summary>
        /// The list of tracked methods
        /// </summary>
        public TrackedMethodRef[] TrackedMethodRefs
        {
            get
            {
                return _tracked != null ? _tracked.ToArray() : null;
            }
            set
            {
                _tracked = null;
                if (value == null) return;
                _tracked = new List<TrackedMethodRef>(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public UInt32 OrigSequencePoint { get; set; }
    }
}
