using System;
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
            _manager.Initialise("Local", "C#", new String[0]);
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
            uint bufferId;
            _manager.AllocateMemoryBuffer(100, out bufferId);
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.IsTrue(_manager.GetBlocks.First().Active);

            // act
            _manager.DeactivateMemoryBuffer(bufferId);

            // assert
            Assert.IsFalse(_manager.GetBlocks.First().Active);
        }

        [Test]
        public void RemoveDeactivatedBlocks_RemovesAllBlocks_ThatHaveActiveFalse()
        {
            // arrange
            uint bufferId;
            _manager.AllocateMemoryBuffer(100, out bufferId);
            _manager.AllocateMemoryBuffer(100, out bufferId);
            Assert.AreEqual(2, _manager.GetBlocks.Count);
            Assert.AreEqual(2, _manager.GetBlocks.Count(b => b.Active));
            _manager.DeactivateMemoryBuffer(bufferId);
            Assert.AreEqual(2, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));
            
            // act
            _manager.RemoveDeactivatedBlocks();

            // assert
            Assert.AreEqual(1, _manager.GetBlocks.Count);
            Assert.AreEqual(1, _manager.GetBlocks.Count(b => b.Active));
        }
    }
}