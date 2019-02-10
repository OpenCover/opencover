using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using log4net;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// Manages the blocks used for communcation and data between host and profiler
    /// </summary>
    public class MemoryManager : IMemoryManager
    {
        private string _namespace;
        private string _key;
        private readonly object _lockObject = new object();
        private uint _bufferId = 1;

        private static readonly ILog DebugLogger = LogManager.GetLogger("DebugLogger");

        private readonly IList<ManagedBufferBlock> _blocks = new List<ManagedBufferBlock>();

        /// <summary>
        /// 
        /// </summary>
        public class ManagedBlock
        {
            /// <summary>
            /// </summary>
            protected string Namespace;

            /// <summary>
            /// </summary>
            protected string Key;

            /// <summary>
            /// Create a unique name
            /// </summary>
            /// <param name="name"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            protected string MakeName(string name, int id)
            {
                var newName = string.Format("{0}{1}{2}{3}", Namespace, name, Key, id);
                return newName;
            }
        }

        /// <summary>
        /// Contain the memory map and synchronisation objects for result communication
        /// </summary>
        internal class ManagedMemoryBlock : ManagedBlock, IManagedMemoryBlock
        {
            public EventWaitHandle ProfilerHasResults { get; private set; }
            public EventWaitHandle ResultsHaveBeenReceived { get; private set; }
            private readonly MemoryMappedFile _mmfResults;
            public MemoryMappedViewStream StreamAccessorResults { get; private set; }
            public int BufferSize { get; private set; }
            public byte[] Buffer { get; private set; }

            private readonly Semaphore _semaphore;

            /// <summary>
            /// Gets an ACL for unit test purposes
            /// </summary>
            internal MemoryMappedFileSecurity MemoryAcl
            {
                get { return _mmfResults.GetAccessControl(); }
            }

            internal ManagedMemoryBlock(string @namespace, string key, int bufferSize, int bufferId,
                IEnumerable<string> servicePrincpal)
            {
                Namespace = @namespace;
                Key = key;

                EventWaitHandleSecurity handleSecurity = null;
                MemoryMappedFileSecurity memSecurity = null;
                SemaphoreSecurity semaphoreSecurity = null;

                var serviceIdentity = servicePrincpal.FirstOrDefault();
                var currentIdentity = WindowsIdentity.GetCurrent();
                if (serviceIdentity != null && currentIdentity != null)
                {
                    handleSecurity = new EventWaitHandleSecurity();
                    handleSecurity.AddAccessRule(new EventWaitHandleAccessRule(currentIdentity.Name,
                        EventWaitHandleRights.FullControl, AccessControlType.Allow));

                    // The event handles need more than just EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize to work
                    handleSecurity.AddAccessRule(new EventWaitHandleAccessRule(serviceIdentity,
                        EventWaitHandleRights.FullControl, AccessControlType.Allow));

                    memSecurity = new MemoryMappedFileSecurity();
                    memSecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>(currentIdentity.Name,
                        MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                    memSecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>(serviceIdentity,
                        MemoryMappedFileRights.ReadWrite, AccessControlType.Allow));

                    semaphoreSecurity = new SemaphoreSecurity();
                    semaphoreSecurity.AddAccessRule(new SemaphoreAccessRule(currentIdentity.Name,
                        SemaphoreRights.FullControl, AccessControlType.Allow));
                    semaphoreSecurity.AddAccessRule(new SemaphoreAccessRule(serviceIdentity, SemaphoreRights.FullControl,
                        AccessControlType.Allow));
                }

                bool createdNew;

                ProfilerHasResults = new EventWaitHandle(
                    false,
                    EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Results_SendResults_Event_", bufferId),
                    out createdNew,
                    handleSecurity);

                ResultsHaveBeenReceived = new EventWaitHandle(
                    false,
                    EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Results_ReceiveResults_Event_", bufferId),
                    out createdNew,
                    handleSecurity);

                _semaphore = new Semaphore(0, 2,
                    MakeName(@"\OpenCover_Profiler_Results_Semaphore_", bufferId),
                    out createdNew,
                    semaphoreSecurity);

                _mmfResults = MemoryMappedFile.CreateNew(
                    MakeName(@"\OpenCover_Profiler_Results_MemoryMapFile_", bufferId),
                    bufferSize,
                    MemoryMappedFileAccess.ReadWrite,
                    MemoryMappedFileOptions.None,
                    memSecurity,
                    HandleInheritability.Inheritable);

                Buffer = new byte[bufferSize];
                StreamAccessorResults = _mmfResults.CreateViewStream(0, bufferSize, MemoryMappedFileAccess.ReadWrite);
                StreamAccessorResults.Write(BitConverter.GetBytes(0), 0, 4);
                StreamAccessorResults.Flush();

                BufferSize = bufferSize;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            private bool _disposed;
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed && disposing)
                {
                    _disposed = true;
                    _semaphore
                        .Try(s => s.Release(1))
                        .Do(s => s.Dispose());
                    ProfilerHasResults.Do(e => e.Dispose());
                    ResultsHaveBeenReceived.Do(e => e.Dispose());
                    StreamAccessorResults.Do(r => r.Dispose());
                    _mmfResults.Do(r => r.Dispose());
                }
            }
        }

        /// <summary>
        /// Contain the memory map and synchronisation objects for request communication
        /// </summary>
        internal class ManagedCommunicationBlock : ManagedBlock, IManagedCommunicationBlock
        {
            private readonly MemoryMappedFile _memoryMappedFile;
            public MemoryMappedViewStream StreamAccessorComms { get; private set; }
            public EventWaitHandle ProfilerRequestsInformation { get; private set; }
            public EventWaitHandle InformationReadyForProfiler { get; private set; }
            public EventWaitHandle InformationReadByProfiler { get; private set; }
            public byte[] DataCommunication { get; private set; }
            public GCHandle PinnedDataCommunication { get; private set; }

            private readonly Semaphore _semaphore;

            /// <summary>
            /// Gets an ACL for unit test purposes
            /// </summary>
            internal MemoryMappedFileSecurity MemoryAcl
            {
                get { return _memoryMappedFile.GetAccessControl(); }
            }

            internal ManagedCommunicationBlock(string @namespace, string key, int bufferSize, int bufferId,
                IEnumerable<string> servicePrincpal)
            {
                Namespace = @namespace;
                Key = key;

                EventWaitHandleSecurity eventSecurity = null;
                MemoryMappedFileSecurity memorySecurity = null;

                var serviceIdentity = servicePrincpal.FirstOrDefault();
                var currentIdentity = WindowsIdentity.GetCurrent();
                SemaphoreSecurity semaphoreSecurity = null;
                if (serviceIdentity != null && currentIdentity != null)
                {
                    eventSecurity = new EventWaitHandleSecurity();
                    eventSecurity.AddAccessRule(new EventWaitHandleAccessRule(currentIdentity.Name,
                        EventWaitHandleRights.FullControl, AccessControlType.Allow));
                    eventSecurity.AddAccessRule(new EventWaitHandleAccessRule(serviceIdentity,
                        EventWaitHandleRights.FullControl, AccessControlType.Allow));

                    memorySecurity = new MemoryMappedFileSecurity();
                    memorySecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>(currentIdentity.Name,
                        MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                    memorySecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>(serviceIdentity,
                        MemoryMappedFileRights.ReadWrite, AccessControlType.Allow));

                    semaphoreSecurity = new SemaphoreSecurity();
                    semaphoreSecurity.AddAccessRule(new SemaphoreAccessRule(currentIdentity.Name,
                        SemaphoreRights.FullControl, AccessControlType.Allow));
                    semaphoreSecurity.AddAccessRule(new SemaphoreAccessRule(serviceIdentity, SemaphoreRights.FullControl,
                        AccessControlType.Allow));
                }

                bool createdNew;

                ProfilerRequestsInformation = new EventWaitHandle(
                    false,
                    EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_SendData_Event_", bufferId),
                    out createdNew,
                    eventSecurity);

                InformationReadyForProfiler = new EventWaitHandle(
                    false,
                    EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_ReceiveData_Event_", bufferId),
                    out createdNew,
                    eventSecurity);

                InformationReadByProfiler = new EventWaitHandle(
                    false,
                    EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_ChunkData_Event_", bufferId),
                    out createdNew,
                    eventSecurity);

                _semaphore = new Semaphore(0, 2,
                    MakeName(@"\OpenCover_Profiler_Communication_Semaphore_", bufferId),
                    out createdNew,
                    semaphoreSecurity);

                _memoryMappedFile = MemoryMappedFile.CreateNew(
                    MakeName(@"\OpenCover_Profiler_Communication_MemoryMapFile_", bufferId),
                    bufferSize,
                    MemoryMappedFileAccess.ReadWrite,
                    MemoryMappedFileOptions.None,
                    memorySecurity,
                    HandleInheritability.Inheritable);

                StreamAccessorComms = _memoryMappedFile.CreateViewStream(0, bufferSize, MemoryMappedFileAccess.ReadWrite);

                DataCommunication = new byte[bufferSize];
                PinnedDataCommunication = GCHandle.Alloc(DataCommunication, GCHandleType.Pinned);
            }

            public void Dispose()
            {
                Dispose(true);
            }

            private bool _disposed;
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed && disposing)
                {
                    _disposed = true;
                    _semaphore
                        .Try(s => s.Release(1))
                        .Do(s => s.Dispose());
                    ProfilerRequestsInformation.Do(e => e.Dispose());
                    InformationReadyForProfiler.Do(e => e.Dispose());
                    InformationReadByProfiler.Do(e => e.Dispose());
                    StreamAccessorComms.Do(r => r.Dispose());
                    _memoryMappedFile.Do(f => f.Dispose());
                    PinnedDataCommunication.Free();
                }
            }
        }

        private bool _isIntialised;

        private string[] _servicePrincipal;

        /// <summary>
        /// Initialise the memory manager
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="key"></param>
        /// <param name="servicePrincipal"></param>
        public void Initialise(string @namespace, string key, IEnumerable<string> servicePrincipal)
        {
            lock (_lockObject)
            {
                if (_isIntialised)
                    return;
                _namespace = @namespace;
                _key = key;
                _servicePrincipal = servicePrincipal.ToArray();
                _isIntialised = true;
            }
        }

        /// <summary>
        /// Allocate a memory buffer
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="bufferId"></param>
        /// <returns></returns>
        public ManagedBufferBlock AllocateMemoryBuffer(int bufferSize, out uint bufferId)
        {
            bufferId = 0;

            lock (_lockObject)
            {
                if (!_isIntialised)
                    return null;
                bufferId = _bufferId++;
                var tuple = new ManagedBufferBlock
                {
                    CommunicationBlock =
                        new ManagedCommunicationBlock(_namespace, _key, bufferSize, (int) bufferId, _servicePrincipal),
                    MemoryBlock =
                        new ManagedMemoryBlock(_namespace, _key, bufferSize, (int) bufferId, _servicePrincipal),
                    BufferId = bufferId
                };
                _blocks.Add(tuple);
                return tuple;
            }
        }


        /// <summary>
        /// Get the list of all allocated blocks
        /// </summary>
        public IReadOnlyList<ManagedBufferBlock> GetBlocks
        {
            get
            {
                lock (_lockObject)
                {
                    return _blocks.ToArray();
                }
            }
        }

        /// <summary>
        /// deactivate a memory block
        /// </summary>
        /// <param name="bufferId"></param>
        public void DeactivateMemoryBuffer(uint bufferId)
        {
            lock (_lockObject)
            {
                var block = _blocks.FirstOrDefault(b => b.BufferId == bufferId);
                if (block == null)
                    return;
                block.Active = false;
            }
        }

        /// <summary>
        /// remove deactivated blocks
        /// </summary>
        public void RemoveDeactivatedBlock(ManagedBufferBlock block)
        {
            lock (_lockObject)
            {
                if (block.Active)
                    return;
                block.CommunicationBlock.Do(x => x.Dispose());
                block.MemoryBlock.Do(x => x.Dispose());
                _blocks.RemoveAt(_blocks.IndexOf(block));
            }
        }

        /// <summary>
        /// Wait for the blocks to close
        /// </summary>
        /// <param name="bufferWaitCount"></param>
        public void WaitForBlocksToClose(int bufferWaitCount)
        {
            // we need to let the profilers dump the thread buffers over before they close - max 15s (ish)
            var i = 0;
            var count = -1;
            while (i < bufferWaitCount && count != 0)
            {
                lock (_lockObject)
                {
                    count = _blocks.Count(b => b.Active);
                }
                if (count > 0)
                {
                    DebugLogger.InfoFormat("Waiting for {0} processes to close", count);
                    Thread.Sleep(500);
                }
                i++;
            }
        }

        /// <summary>
        /// fetch remaining buffer data
        /// </summary>
        /// <param name="processBuffer"></param>
        public void FetchRemainingBufferData(Action<byte[]> processBuffer)
        {
            lock (_lockObject)
            {
                // grab anything left in the main buffers
                var activeBlocks = _blocks.Where(b => b.Active).ToArray();
                foreach (var block in activeBlocks)
                {
                    var memoryBlock = block.MemoryBlock;
                    var data = new byte[memoryBlock.BufferSize];
                    memoryBlock.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                    memoryBlock.StreamAccessorResults.Read(data, 0, memoryBlock.BufferSize);

                    // process the extracted data
                    processBuffer(data);

                    // now clean them down
                    block.CommunicationBlock.Do(x => x.Dispose());
                    block.MemoryBlock.Do(x => x.Dispose());
                    _blocks.RemoveAt(_blocks.IndexOf(block));
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                lock (_lockObject)
                {
                    foreach (var block in _blocks)
                    {
                        block.CommunicationBlock.Do(x => x.Dispose());
                        block.MemoryBlock.Do(x => x.Dispose());
                    }
                    _blocks.Clear();
                }
            }
        }
    }
}
