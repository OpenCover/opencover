using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
        private MemoryMappedViewStream _streamAccessorComms;
        private MemoryMappedViewStream _streamAccessorResults;
        private EventWaitHandle _requestDataReady;
        private EventWaitHandle _responseDataReady;
        private EventWaitHandle _requestResultsReady;
        private EventWaitHandle _responseResultsReady;
        private byte[] _dataCommunication;
        private byte[] _dataResults;

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

            _requestResultsReady = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\OpenCover_Profiler_Communication_SendResults_Event_" + key);
            _responseResultsReady = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\OpenCover_Profiler_Communication_ReceiveResults_Event_" + key);

            handles.Add(_requestResultsReady);

            using (var mmfComms = MemoryMappedFile.CreateNew(@"Local\OpenCover_Profiler_Communication_MemoryMapFile_" + key, maxMsgSize))
            using (var mmfResults = MemoryMappedFile.CreateNew(@"Local\OpenCover_Profiler_Results_MemoryMapFile_" + key, maxMsgSize))
            using (_streamAccessorComms = mmfComms.CreateViewStream(0, maxMsgSize, MemoryMappedFileAccess.ReadWrite))
            using (_streamAccessorResults = mmfResults.CreateViewStream(0, maxMsgSize, MemoryMappedFileAccess.ReadWrite))
            {
                _streamAccessorResults.Write(BitConverter.GetBytes(0), 0, 4);
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

                _dataCommunication = new byte[maxMsgSize];
                _dataResults = new byte[maxMsgSize];
                var pinnedComms = GCHandle.Alloc(_dataCommunication, GCHandleType.Pinned);
                var pinnedResults = GCHandle.Alloc(_dataResults, GCHandleType.Pinned);
                try
                {
                    ProcessMessages(handles, pinnedComms, pinnedResults);
                }
                finally
                {
                    pinnedComms.Free();
                    pinnedResults.Free();
                    
                }
            }
        }

        private void ProcessMessages(List<WaitHandle> handles, GCHandle pinnedComms, GCHandle pinnedResults)
        {
            var continueWait = true;
            do
            {
                switch (WaitHandle.WaitAny(handles.ToArray()))
                {
                    case 1:
                        _requestDataReady.Reset();
                                            
                        _streamAccessorComms.Seek(0, SeekOrigin.Begin);
                        _streamAccessorComms.Read(_dataCommunication, 0, _messageHandler.ReadSize);

                        var writeSize = _messageHandler.StandardMessage(
                            (MSG_Type)BitConverter.ToInt32(_dataCommunication, 0), 
                            pinnedComms.AddrOfPinnedObject(), 
                            SendChunkAndWaitForConfirmation);
                            
                        _streamAccessorComms.Seek(0, SeekOrigin.Begin);
                        _streamAccessorComms.Write(_dataCommunication, 0, writeSize);

                        _responseDataReady.Set();
                        _responseDataReady.Reset();

                        break;
                    case 2:
                        _requestResultsReady.Reset();

                        _streamAccessorResults.Seek(0, SeekOrigin.Begin);
                        _streamAccessorResults.Read(_dataResults, 0, maxMsgSize);

                        _responseResultsReady.Set();
                        _responseResultsReady.Reset();

                        _messageHandler.ReceiveResults(pinnedResults.AddrOfPinnedObject());

                        break;
                    default:
                        continueWait = false;
                        break;
                }
            } while (continueWait);

            _streamAccessorResults.Seek(0, SeekOrigin.Begin);
            _streamAccessorResults.Read(_dataResults, 0, maxMsgSize);

            _messageHandler.ReceiveResults(pinnedResults.AddrOfPinnedObject());

            _messageHandler.Complete();

        }

        private void SendChunkAndWaitForConfirmation(int writeSize)
        {
            _streamAccessorComms.Seek(0, SeekOrigin.Begin);
            _streamAccessorComms.Write(_dataCommunication, 0, writeSize);
            
            _requestDataReady.Reset();
            WaitHandle.SignalAndWait(_responseDataReady, _requestDataReady);
            _responseDataReady.Reset();
        }
    }
}
