using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// Defines the buffer between the host and a profiler instance 
    /// </summary>
    public class ManagedBufferBlock
    {
        /// <summary>
        /// Defines the buffer between the host and a profiler instance 
        /// </summary>
        public ManagedBufferBlock()
        {
            Active = true;
        }

        /// <summary>
        /// A communication block is where all commands are sent from profiler to host 
        /// </summary>
        public IManagedCommunicationBlock CommunicationBlock { get; set; }

        /// <summary>
        /// A memory block is were the results are sent
        /// </summary>
        public IManagedMemoryBlock MemoryBlock { get; set; }

        /// <summary>
        /// The buffer identifier
        /// </summary>
        public uint BufferId { get; set; }

        /// <summary>
        /// Is the block still active?
        /// </summary>
        public bool Active { get; set; }
    }

    /// <summary>
    /// Defines the interface for a memory manager implementation
    /// </summary>
    public interface IMemoryManager : IDisposable
    {
        /// <summary>
        /// Initialise a memory manager
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="key"></param>
        /// <param name="servicePrincipal"></param>
        void Initialise(string nameSpace, string key, IEnumerable<string> servicePrincipal);

        /// <summary>
        /// Allocate a <see cref="ManagedBufferBlock"/> that is used to communicate between host and profiler
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="bufferId"></param>
        /// <returns></returns>
        ManagedBufferBlock AllocateMemoryBuffer(int bufferSize, out uint bufferId);

        /// <summary>
        /// Get the list of all allocated blocks
        /// </summary>
        IReadOnlyList<ManagedBufferBlock> GetBlocks { get; }

        /// <summary>
        /// </summary>
        /// <param name="bufferId"></param>
        void DeactivateMemoryBuffer(uint bufferId);
        
        /// <summary>
        /// Remove all deactivated blocks
        /// </summary>
        void RemoveDeactivatedBlock(ManagedBufferBlock block);

        /// <summary>
        /// Wait some time for the blocks to close
        /// </summary>
        /// <param name="bufferWaitCount"></param>
        void WaitForBlocksToClose(int bufferWaitCount);

        /// <summary>
        /// Fetch the remaining data from the active blocks
        /// </summary>
        /// <param name="processBuffer"></param>
        void FetchRemainingBufferData(Action<byte[]> processBuffer);
    }
}