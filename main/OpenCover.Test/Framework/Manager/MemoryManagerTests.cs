using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using OpenCover.Framework.Manager;

namespace OpenCover.Test.Framework.Manager
{
    [TestFixture]
    public class MemoryManagerTests
    {
        private MemoryManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new MemoryManager();
            _manager.Initialise("Local", "C#", new string[0]);
        }

        [TearDown]
        public void Teardown()
        {
            _manager.Dispose();
        }

        [Test]
        public void DeactivateMemoryBuffer_SetsActive_ForBlock_False()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out var bufferId);
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.IsTrue(_manager.GetBlocks.First().Active);

            // act
            _manager.DeactivateMemoryBuffer(bufferId);

            // assert
            Assert.IsFalse(_manager.GetBlocks.First().Active);
        }

        [Test]
        public void RemoveDeactivatedBlocs_RemovesNonActiveBlock()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out var bufferId);
            _manager.AllocateMemoryBuffer(100, out bufferId);
            Assert.AreEqual(2, _manager.GetBlocks.Count);
            Assert.AreEqual(2, _manager.GetBlocks.Count(b => b.Active));
            _manager.DeactivateMemoryBuffer(bufferId);
            Assert.AreEqual(2, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));
            
            // act
            var block = _manager.GetBlocks.First(b => !b.Active);
            _manager.RemoveDeactivatedBlock(block);

            // assert
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));
        }

        [Test]
        public void Cannot_RemoveDeactivatedBlock_OnActiveBlock()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out _);
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));

            // act
            var block = _manager.GetBlocks.First();
            _manager.RemoveDeactivatedBlock(block);

            // assert
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));
        }

        [Test]
        public void AllocateMemoryBuffer_WhenManagerNotInitialised_Ignored_OK()
        {
            using (var manager = new MemoryManager())
            {
                // not initialised

                // arrange

                // act & assert
                Assert.That(() => manager.AllocateMemoryBuffer(100, out _), Throws.Nothing);
            }
        }

        [Test]
        public void InitialiseMemoryManagerTwice_Ignored_OK()
        {
            // act & assert
            Assert.That(() => _manager.Initialise("Local", "C#", new String[0]), Throws.Nothing);
        }

        [Test]
        public void AllocateMemoryBufferTwice_NewBufferAllocated_OK()
        {
            // arrange

            // act
            _manager.AllocateMemoryBuffer(100, out _);
            _manager.AllocateMemoryBuffer(100, out _);

            // assert
            Assert.AreEqual(2, _manager.GetBlocks.Count);
        }

        [Test]
        public void DeactivateMemoryBufferTwice_Ignored_OK()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out var bufferId);
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.IsTrue(_manager.GetBlocks.First().Active);

            // act
            _manager.DeactivateMemoryBuffer(bufferId);

            // act & assert
            Assert.That(() => _manager.DeactivateMemoryBuffer(bufferId), Throws.Nothing);
        }


        [Test]
        public void DeactivateMemoryBufferAfterDisposed_Ignored_OK()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out var bufferId);
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.IsTrue(_manager.GetBlocks.First().Active);

            // act
            _manager.Dispose();

            // act & assert
            Assert.That(() => _manager.DeactivateMemoryBuffer(bufferId), Throws.Nothing);
        }

        [Test]
        public void WaitForBlocksToClose_WaitsUntilBufferWaitCountExceededIfAnyActiveBlocks()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out _);

            var timeAction = new Func<Action, long>(actionToTime =>
            {
                var t = Stopwatch.StartNew();
                actionToTime();
                t.Stop();
                return t.ElapsedMilliseconds;
            });

            Assert.That(timeAction(() => _manager.WaitForBlocksToClose(0)), Is.LessThan(500));
            Assert.That(timeAction(() => _manager.WaitForBlocksToClose(1)), Is.GreaterThanOrEqualTo(500).And.LessThan(1000));
            Assert.That(timeAction(() => _manager.WaitForBlocksToClose(2)), Is.GreaterThanOrEqualTo(1000).And.LessThan(1500));

        }

        [Test]
        public void WaitForBlocksToClose_StopsWaitingWhenNoActiveBlocks()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out _);

            var timeAction = new Func<Action, long>(actionToTime =>
            {
                var t = Stopwatch.StartNew();
                actionToTime();
                t.Stop();
                return t.ElapsedMilliseconds;
            });

            Assert.That(timeAction(() => _manager.WaitForBlocksToClose(1)), Is.GreaterThanOrEqualTo(500).And.LessThan(1000));
            _manager.GetBlocks.First().Active = false;
            Assert.That(timeAction(() => _manager.WaitForBlocksToClose(1)), Is.LessThan(500));

        }

        [Test]
        public void FetchRemainingBufferData_CallsActionForEachActiveBlock()
        {
            // arrange
            _manager.AllocateMemoryBuffer(100, out _);
            _manager.AllocateMemoryBuffer(100, out _);

            uint count = 0;
            _manager.FetchRemainingBufferData( data => count++ );

            Assert.That(count, Is.EqualTo(2));
            Assert.That(_manager.GetBlocks.Count, Is.EqualTo(0));
        }
    }
}