/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 9.1.2016.
 * Time: 15:11
 * 
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Utility;

namespace OpenCover.Test.Framework.Utility
{
    [TestFixture]
    public class SequencePointComparerTest
    {
        SequencePointComparer comparer = new SequencePointComparer();

        [Test]
        public void DoesNotEqualNull()
        {
            var point = new SequencePoint();

            Assert.IsFalse(comparer.Equals(point, null));
        }

        [Test]
        public void DoesEqualSelf()
        {
            var point = new SequencePoint();

            Assert.IsTrue(comparer.Equals(point, point));
        }

        [Test]
        public void DoesEqualSimilar()
        {
            var point1 = new SequencePoint {FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1};
            var point2 = new SequencePoint {FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1};

            Assert.IsTrue(comparer.Equals(point1, point2));
        }

        [Test]
        [TestCase(0, 1, 1, 1, 1)]
        [TestCase(1, 2, 1, 1, 1)]
        [TestCase(1, 1, 3, 1, 1)]
        [TestCase(1, 1, 1, 4, 1)]
        [TestCase(1, 1, 1, 1, 5)]
        public void DoesNotEqualDisimilar(int fileId, int startLine, int startColumn, int endLine, int endColumn)
        {
            var point1 = new SequencePoint { FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1 };
            var point2 = new SequencePoint
            {
                FileId = (uint)fileId,
                StartLine = startLine,
                StartColumn = startColumn,
                EndLine = endLine,
                EndColumn = endColumn
            };

            Assert.IsFalse(comparer.Equals(point1, point2));
            
        }

        [Test]
        public void UsageThatCoversGetHashCode() {

            var sequencePointsSet = new HashSet<SequencePoint>(comparer);
            var point1 = new SequencePoint {FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1};
            var point2 = new SequencePoint {FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1};
            var point3 = new SequencePoint {FileId = 2, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 1};

            Assert.True (sequencePointsSet.Add(point1));
            Assert.False (sequencePointsSet.Add(point1));

            Assert.True (sequencePointsSet.Contains(point2));
            Assert.False (sequencePointsSet.Add(point2));

            Assert.False (sequencePointsSet.Contains(point3));
            Assert.True (sequencePointsSet.Add(point3));

        }
    }
}
