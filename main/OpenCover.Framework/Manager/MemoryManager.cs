using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenCover.Framework.Manager
{
    public class MemoryManager : IMemoryManager
    {
        private string _namespace;
        private string _key;
        private object lockObject = new object();

        private IList<ManagedMemoryBlock> _blocks = new List<ManagedMemoryBlock>(); 

        public class ManagedMemoryBlock
        {
            public EventWaitHandle ProfilerHasResults { get; internal set; }
            public EventWaitHandle ResultsHaveBeenReceived { get; internal set; }
            public MemoryMappedFile MMFResults { get; internal set; }
            public MemoryMappedViewStream StreamAccessorResults { get; internal set; }
            public int BufferSize { get; internal set; }
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
                var block = new ManagedMemoryBlock();
                block.ProfilerHasResults = new EventWaitHandle(false, EventResetMode.ManualReset, MakeName(@"\OpenCover_Profiler_Communication_SendResults_Event_", bufferId));
                block.ResultsHaveBeenReceived = new EventWaitHandle(false, EventResetMode.ManualReset, MakeName(@"\OpenCover_Profiler_Communication_ReceiveResults_Event_", bufferId));
                block.MMFResults = MemoryMappedFile.CreateNew(MakeName(@"\OpenCover_Profiler_Results_MemoryMapFile_", bufferId), bufferSize);
                block.StreamAccessorResults = block.MMFResults.CreateViewStream(0, bufferSize, MemoryMappedFileAccess.ReadWrite);
                block.StreamAccessorResults.Write(BitConverter.GetBytes(0), 0, 4);
                block.BufferSize = bufferSize;

                _blocks.Add(block);
            }
        }

        public IList<WaitHandle> GetHandles()
        {
            lock (lockObject)
            {
                return _blocks.Select(x => x.ProfilerHasResults).ToList<WaitHandle>();
            }
        }

        public IList<ManagedMemoryBlock> GetBlocks
        {
            get { return _blocks; }
        }

        private string MakeName(string name, long id)
        {
            return string.Format("{0}{1}{2}{3}", _namespace, name, _key, id);
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                foreach (var managedMemoryBlock in _blocks)
                {
                    managedMemoryBlock.StreamAccessorResults.Dispose();
                    managedMemoryBlock.MMFResults.Dispose();
                }
                _blocks.Clear();
            }
        }
    }
}
