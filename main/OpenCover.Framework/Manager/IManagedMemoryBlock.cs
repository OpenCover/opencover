using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// Define a results communication interface
    /// </summary>
    public interface IManagedMemoryBlock : IDisposable
    {
        /// <summary>
        /// Signalled by profiler when the profiler has results
        /// </summary>
        EventWaitHandle ProfilerHasResults { get; }

        /// <summary>
        /// Signalled by host when results ahve been read
        /// </summary>
        EventWaitHandle ResultsHaveBeenReceived { get; }

        /// <summary>
        /// Access the results
        /// </summary>
        MemoryMappedViewStream StreamAccessorResults { get; }

        /// <summary>
        /// Get the buffer size
        /// </summary>
        int BufferSize { get; }

        /// <summary>
        /// Get the buffer
        /// </summary>
        byte[] Buffer { get; }
    }
}