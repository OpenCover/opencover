using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenCover.Framework.Model;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    public class SequencePointTests
    {
        [Test]
        public void When_AddingLotsOfSequencePoints_Array_GrowsAutomatically()
        {
            // arrange

            // act
            for (int i = 0; i < 10000; i++)
            {
                new SequencePoint();
            }

            // assert
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(10000, 0, 100));

            Assert.AreEqual(100, InstrumentationPoint.GetVisitCount(10000));

        }

        // TODO: Add tests around Instrumentation point
        [Test]
        public void When_AccessingInstrumentationPoints_OutsideAllowedRange()
        {
            // arrange

            // act
            for (int i = 0; i < 10; i++)
            {
                new SequencePoint();
            }

            // assert
            Assert.IsFalse(InstrumentationPoint.AddVisitCount(0, 0, 100));
            Assert.IsFalse(InstrumentationPoint.AddVisitCount(1000000, 0, 100));
        }

        [Test]
        public void VisitCountIsLimitedToMax()
        {
            // arrange
            InstrumentationPoint.Clear();

            // act
            for (int i = 0; i < 10; i++)
            {
                new SequencePoint();
            }

            // assert
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 0, 100));
            Assert.AreEqual(100, InstrumentationPoint.GetVisitCount(1));
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 0, int.MaxValue));
            Assert.AreEqual(int.MaxValue, InstrumentationPoint.GetVisitCount(1));
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 0, 300));
            Assert.AreEqual(int.MaxValue, InstrumentationPoint.GetVisitCount(1));
        }

        [Test]
        public void TrackedMethodCountIsLimitedToMax()
        {
            // arrange
            InstrumentationPoint.Clear();

            var list = new List<SequencePoint>();

            // act
            for (var i = 0; i < 10; i++)
            {
               list.Add(new SequencePoint());
            }

            // assert
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 1, 100));
            Assert.AreEqual(100, list.First(x => x.UniqueSequencePoint == 1).TrackedMethodRefs.First(x => x.UniqueId == 1).VisitCount);
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 1, int.MaxValue));
            Assert.AreEqual(int.MaxValue, list.First(x => x.UniqueSequencePoint == 1).TrackedMethodRefs.First(x => x.UniqueId == 1).VisitCount);
            Assert.IsTrue(InstrumentationPoint.AddVisitCount(1, 1, 200));
            Assert.AreEqual(int.MaxValue, list.First(x => x.UniqueSequencePoint == 1).TrackedMethodRefs.First(x => x.UniqueId == 1).VisitCount);
        }

        [Test]
        public void CanDetermineSingleCharSequencePoint()
        {
            Assert.IsTrue(new SequencePoint { FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 2 }.IsSingleCharSequencePoint);
            Assert.IsFalse(new SequencePoint { FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 3 }.IsSingleCharSequencePoint);
            Assert.IsFalse(new SequencePoint { FileId = 1, StartLine = 1, StartColumn = 1, EndLine = 2, EndColumn = 2 }.IsSingleCharSequencePoint);
        }
    }
}
