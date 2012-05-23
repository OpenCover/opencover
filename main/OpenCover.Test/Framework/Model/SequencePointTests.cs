using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
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
    }
}
