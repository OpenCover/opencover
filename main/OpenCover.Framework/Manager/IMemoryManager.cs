using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public interface IMemoryManager : IDisposable
    {
        void Initialise(string nameSpace, string key);
        void AllocateMemoryBuffer(int bufferSize, uint bufferId);
        IList<WaitHandle> GetHandles();
        IList<MemoryManager.ManagedMemoryBlock> GetBlocks { get; }
    }
}