using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCover.Framework.Communication;

namespace OpenCover.Framework.Manager
{
    public class ProfilerManager : IProfilerManager
    {
        const int maxMsgSize = 65536;

        private readonly IMessageHandler _messageHandler;
        private MemoryMappedViewStream _streamAccessor;
        private EventWaitHandle _requestDataReady;
        private EventWaitHandle _responseDataReady;
        private byte[] _data;

        public ProfilerManager(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void RunProcess(Action<Action<StringDictionary>> process)
        {
            var key = Guid.NewGuid().GetHashCode().ToString("X");
            var processMgmt = new AutoResetEvent(false);
            var environmentKeyRead = new AutoResetEvent(false);
            var handles = new List<WaitHandle> { processMgmt };

            _requestDataReady = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\OpenCover_Profiler_Communication_SendData_Event_" + key);
            _responseDataReady = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\OpenCover_Profiler_Communication_ReceiveData_Event_" + key);

            handles.Add(_requestDataReady);
            
            using (var mmf = MemoryMappedFile.CreateNew(@"Local\OpenCover_Profiler_Communication_MemoryMapFile_" + key, maxMsgSize))
            using (_streamAccessor = mmf.CreateViewStream(0, maxMsgSize, MemoryMappedFileAccess.ReadWrite))
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        process(dictionary =>
                        {
                            if (dictionary == null) return;
                            dictionary.Add(@"OpenCover_Profiler_Key", key);
                            environmentKeyRead.Set();
                        });
                    }
                    finally
                    {
                        processMgmt.Set();
                    }
                });

                // wait for the environment key to be read
                if (WaitHandle.WaitAny(new[] { environmentKeyRead }, new TimeSpan(0, 0, 0, 10)) == -1) 
                    return;

                _data = new byte[maxMsgSize];
                var pinned = GCHandle.Alloc(_data, GCHandleType.Pinned);
                try
                {
                    ProcessMessages(handles, pinned);
                }
                finally
                {
                    pinned.Free();                    
                }
            }
        }

        private void ProcessMessages(List<WaitHandle> handles, GCHandle pinned)
        {
            var continueWait = true;
            do
            {
                switch (WaitHandle.WaitAny(handles.ToArray()))
                {
                    case 1:
                        _requestDataReady.Reset();
                                            
                        _streamAccessor.Seek(0, SeekOrigin.Begin);
                        _streamAccessor.Read(_data, 0, _messageHandler.ReadSize);
                            
                        var writeSize = _messageHandler.StandardMessage((MSG_Type)BitConverter.ToInt32(_data, 0), pinned.AddrOfPinnedObject(), this);
                            
                        _streamAccessor.Seek(0, SeekOrigin.Begin);
                        _streamAccessor.Write(_data, 0, writeSize);

                        _responseDataReady.Set();
                        _responseDataReady.Reset();
                        break;

                    default:
                        continueWait = false;
                        break;
                }
            } while (continueWait);
        }

        public void SendChunkAndWaitForConfirmation(int writeSize)
        {
            _streamAccessor.Seek(0, SeekOrigin.Begin);
            _streamAccessor.Write(_data, 0, writeSize);
                                            
            _responseDataReady.Set();
            _responseDataReady.Reset();

            WaitHandle.WaitAny(new[] {_requestDataReady});
        }
    }
}
