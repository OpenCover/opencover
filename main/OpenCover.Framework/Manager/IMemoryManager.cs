using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public interface IMemoryManager : IDisposable
    {
        void Initialise(string nameSpace, string key, IEnumerable<string> servicePrincipal);
        Tuple<IManagedCommunicationBlock, IManagedMemoryBlock> AllocateMemoryBuffer(int bufferSize, uint bufferId);
        IList<Tuple<IManagedCommunicationBlock, IManagedMemoryBlock>> GetBlocks { get; }
    }
}