﻿using System;
using System.Threading;

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

        public UInt32 Ordinal { get; set; }
        public int Offset { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public int VisitCount { get; set; }

        public UInt32 UniqueSequencePoint { get; set; }
    }
}