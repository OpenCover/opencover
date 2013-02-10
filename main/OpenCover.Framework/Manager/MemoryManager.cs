using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public class MemoryManager : IMemoryManager
    {
        private string _namespace;
        private string _key;
        private readonly object lockObject = new object();

        private readonly IList<IManagedMemoryBlock> _blocks = new List<IManagedMemoryBlock>();

        public class ManagedBlock
        {
            protected string Namespace;
            protected string Key;

            protected string MakeName(string name, int id)
            {
                var newName = string.Format("{0}{1}{2}{3}", Namespace, name, Key, id);
                //Console.WriteLine(newName);
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

            internal ManagedMemoryBlock(string @namespace, string key, int bufferSize, int bufferId)
            {
                Namespace = @namespace;
                Key = key;

                ProfilerHasResults = new EventWaitHandle(false, EventResetMode.ManualReset, 
                    MakeName(@"\OpenCover_Profiler_Communication_SendResults_Event_", bufferId));

                ResultsHaveBeenReceived = new EventWaitHandle(false, EventResetMode.ManualReset, 
                    MakeName(@"\OpenCover_Profiler_Communication_ReceiveResults_Event_", bufferId));

                _mmfResults = MemoryMappedFile.CreateNew(MakeName(@"\OpenCover_Profiler_Results_MemoryMapFile_", bufferId), bufferSize);

                StreamAccessorResults = _mmfResults.CreateViewStream(0, bufferSize, MemoryMappedFileAccess.ReadWrite);
                StreamAccessorResults.Write(BitConverter.GetBytes(0), 0, 4);
                BufferSize = bufferSize;
            }

            public void Dispose()
            {
                StreamAccessorResults.Dispose();
                _mmfResults.Dispose();
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

            internal ManagedCommunicationBlock(string @namespace, string key, int bufferSize, int bufferId)
            {
                Namespace = @namespace;
                Key = key;

                _memoryMappedFile = MemoryMappedFile.CreateNew(
                    MakeName(@"\OpenCover_Profiler_Communication_MemoryMapFile_", bufferId), bufferSize);
                StreamAccessorComms = _memoryMappedFile.CreateViewStream(0, bufferSize, MemoryMappedFileAccess.ReadWrite);

                ProfilerRequestsInformation = new EventWaitHandle(false, EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_SendData_Event_", bufferId));

                InformationReadyForProfiler = new EventWaitHandle(false, EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_ReceiveData_Event_", bufferId));

                InformationReadByProfiler = new EventWaitHandle(false, EventResetMode.ManualReset,
                    MakeName(@"\OpenCover_Profiler_Communication_ChunkData_Event_", bufferId));

                DataCommunication = new byte[bufferSize];
                PinnedDataCommunication = GCHandle.Alloc(DataCommunication, GCHandleType.Pinned);
            }

            public void Dispose()
            {
                StreamAccessorComms.Dispose();
                _memoryMappedFile.Dispose();
                PinnedDataCommunication.Free();
            }
        }

        private bool isIntialised = false;
        public void Initialise(string @namespace, string key)
        {
            if (isIntialised) return;
            _namespace = @namespace;
            _key = key;
            isIntialised = true;
        }

        public void AllocateMemoryBuffer(int bufferSize, uint bufferId)
        {
            if (!isIntialised) return;

            lock (lockObject)
            {
                _blocks.Add(new ManagedMemoryBlock(_namespace, _key, bufferSize, (int)bufferId));
            }
        }

        public IList<WaitHandle> GetHandles()
        {
            lock (lockObject)
            {
                return _blocks.Select(x => x.ProfilerHasResults).ToList<WaitHandle>();
            }
        }

        public IList<IManagedMemoryBlock> GetBlocks
        {
            get { return _blocks; }
        }

        public void Dispose()
        {
            //Console.WriteLine("Disposing...");
            lock (lockObject)
            {
                foreach (var managedMemoryBlock in _blocks)
                {
                    managedMemoryBlock.Dispose();
                }
                _blocks.Clear();
            }
        }
    }
}
