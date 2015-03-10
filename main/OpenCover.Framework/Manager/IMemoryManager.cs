using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public class ManagedBufferBlock
    {
        public ManagedBufferBlock()
        {
            Active = true;
        }
        public IManagedCommunicationBlock CommunicationBlock { get; set; }
        public IManagedMemoryBlock MemoryBlock { get; set; }
        public uint BufferId { get; set; }
        public bool Active { get; set; }
    }

    public interface IMemoryManager : IDisposable
    {
        void Initialise(string nameSpace, string key, IEnumerable<string> servicePrincipal);
        ManagedBufferBlock AllocateMemoryBuffer(int bufferSize, uint bufferId);
        IList<ManagedBufferBlock> GetBlocks { get; }
        void DeactivateMemoryBuffer(uint bufferId);
        void RemoveDeactivatedBlocks();
    }
}