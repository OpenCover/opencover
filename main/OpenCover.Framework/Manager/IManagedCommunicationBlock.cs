using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public interface IManagedCommunicationBlock : IDisposable
    {
        MemoryMappedViewStream StreamAccessorComms { get; }
        EventWaitHandle ProfilerRequestsInformation { get; }
        EventWaitHandle InformationReadyForProfiler { get; }
        EventWaitHandle InformationReadByProfiler { get; }
        byte[] DataCommunication { get; }
        GCHandle PinnedDataCommunication { get; }
    }
}