using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public interface IManagedMemoryBlock : IDisposable
    {
        EventWaitHandle ProfilerHasResults { get; }
        EventWaitHandle ResultsHaveBeenReceived { get; }
        MemoryMappedViewStream StreamAccessorResults { get; }
        int BufferSize { get; }
    }
}