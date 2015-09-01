using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// Define a command communication interface
    /// </summary>
    public interface IManagedCommunicationBlock : IDisposable
    {
        /// <summary>
        /// The communication data
        /// </summary>
        MemoryMappedViewStream StreamAccessorComms { get; }

        /// <summary>
        /// Signalled by the profiler when the profiler wants some information
        /// </summary>
        EventWaitHandle ProfilerRequestsInformation { get; }

        /// <summary>
        /// Signalled by the host when the data requested is available
        /// </summary>
        EventWaitHandle InformationReadyForProfiler { get; }

        /// <summary>
        /// signalled by the profiler when te data has been read by the profiler
        /// </summary>
        EventWaitHandle InformationReadByProfiler { get; }

        /// <summary>
        /// The data being communicated
        /// </summary>
        byte[] DataCommunication { get; }

        /// <summary>
        /// A handle to the pinned data
        /// </summary>
        GCHandle PinnedDataCommunication { get; }
    }
}