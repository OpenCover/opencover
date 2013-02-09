//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// This is the core manager for integrating the host the target 
    /// application and the profiler 
    /// </summary>
    /// <remarks>It probably does too much!</remarks>
    public class ProfilerManager : IProfilerManager
    {
        const int maxMsgSize = 65536;

        private readonly IMessageHandler _messageHandler;
        private readonly IPersistance _persistance;
        private readonly IMemoryManager _memoryManager;
        private readonly ICommandLine _commandLine;
        private MemoryMappedViewStream _streamAccessorComms;
        private EventWaitHandle _profilerRequestsInformation;
        private EventWaitHandle _informationReadyForProfiler;
        private EventWaitHandle _informationReadByProfiler;
        private byte[] _dataCommunication;
        private ConcurrentQueue<byte[]> _messageQueue;

        public ProfilerManager(IMessageHandler messageHandler, IPersistance persistance, 
            IMemoryManager memoryManager, ICommandLine commandLine)
        {
            _messageHandler = messageHandler;
            _persistance = persistance;
            _memoryManager = memoryManager;
            _commandLine = commandLine;
        }

        public void RunProcess(Action<Action<StringDictionary>> process, bool isService)
        {
            var key = Guid.NewGuid().GetHashCode().ToString("X");
            var processMgmt = new AutoResetEvent(false);
            var queueMgmt = new AutoResetEvent(false);
            var environmentKeyRead = new AutoResetEvent(false);
            var handles = new List<WaitHandle> { processMgmt };

            string @namespace = isService ? "Global" : "Local";

            _memoryManager.Initialise(@namespace, key);

            _profilerRequestsInformation = new EventWaitHandle(false, EventResetMode.ManualReset, 
                @namespace + @"\OpenCover_Profiler_Communication_SendData_Event_" + key);
            _informationReadyForProfiler = new EventWaitHandle(false, EventResetMode.ManualReset, 
                @namespace + @"\OpenCover_Profiler_Communication_ReceiveData_Event_" + key);
            _informationReadByProfiler = new EventWaitHandle(false, EventResetMode.ManualReset, 
                @namespace + @"\OpenCover_Profiler_Communication_ChunkData_Event_" + key);

            handles.Add(_profilerRequestsInformation);

            _messageQueue = new ConcurrentQueue<byte[]>();

            using (var mmfComms = MemoryMappedFile.CreateNew(@namespace + @"\OpenCover_Profiler_Communication_MemoryMapFile_" + key, maxMsgSize))
            using (_streamAccessorComms = mmfComms.CreateViewStream(0, maxMsgSize, MemoryMappedFileAccess.ReadWrite))
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        process(dictionary =>
                        {
                            if (dictionary == null) return;
                            dictionary[@"OpenCover_Profiler_Key"] = key;
                            dictionary[@"OpenCover_Profiler_Namespace"] = @namespace;
                            dictionary[@"OpenCover_Profiler_Threshold"] = _commandLine.Threshold.ToString(CultureInfo.InvariantCulture);

                            dictionary["Cor_Profiler"] = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
                            dictionary["Cor_Enable_Profiling"] = "1";
                            dictionary["CoreClr_Profiler"] = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
                            dictionary["CoreClr_Enable_Profiling"] = "1";
                            environmentKeyRead.Set();
                        });
                    }
                    finally
                    {
                        processMgmt.Set();
                    }
                });

                ThreadPool.QueueUserWorkItem((state) =>
                {
                    while (true)
                    {
                        byte[] data;
                        while (!_messageQueue.TryDequeue(out data))
                            Thread.Yield();

                        if (data.Length == 0)
                        {
                            _messageHandler.Complete();
                            queueMgmt.Set();
                            return;
                        }
                            
                        _persistance.SaveVisitData(data);
                    }
                });

                // wait for the environment key to be read
                if (WaitHandle.WaitAny(new[] { environmentKeyRead }, new TimeSpan(0, 0, 0, 10)) == -1) 
                    return;

                _dataCommunication = new byte[maxMsgSize];
                var pinnedComms = GCHandle.Alloc(_dataCommunication, GCHandleType.Pinned);
                try
                {
                    ProcessMessages(handles, pinnedComms);
                }
                finally
                {
                    pinnedComms.Free();
                }

                queueMgmt.WaitOne();
            }
        }

        private void ProcessMessages(List<WaitHandle> handles, GCHandle pinnedComms)
        {
            var continueWait = true;
            do
            {
                var @events = new List<WaitHandle>(handles);
                @events.AddRange(_memoryManager.GetHandles());

                var @case = WaitHandle.WaitAny(@events.ToArray());
                switch (@case)
                {
                    case 0:
                        continueWait = false;
                        break;
                    case 1:
                        _profilerRequestsInformation.Reset();
                   
                        _streamAccessorComms.Seek(0, SeekOrigin.Begin);
                        _streamAccessorComms.Read(_dataCommunication, 0, _messageHandler.ReadSize);

                        var writeSize = _messageHandler.StandardMessage(
                            (MSG_Type)BitConverter.ToInt32(_dataCommunication, 0), 
                            pinnedComms.AddrOfPinnedObject(), 
                            SendChunkAndWaitForConfirmation);

                        SendChunkAndWaitForConfirmation(writeSize);
                        break;
                    default:
                        var block = _memoryManager.GetBlocks[@case - 2];
                        var data = new byte[block.BufferSize];
                        block.ProfilerHasResults.Reset();

                        block.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                        block.StreamAccessorResults.Read(data, 0, block.BufferSize);

                        block.ResultsHaveBeenReceived.Set();
                        _messageQueue.Enqueue(data);
                        break;
                }
            } while (continueWait);

            foreach (var block in _memoryManager.GetBlocks)
            {
                var data = new byte[block.BufferSize];
                block.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                block.StreamAccessorResults.Read(data, 0, block.BufferSize);
                _messageQueue.Enqueue(data);    
            }

            _messageQueue.Enqueue(new byte[0]);
        }

        private void SendChunkAndWaitForConfirmation(int writeSize)
        {
            _streamAccessorComms.Seek(0, SeekOrigin.Begin);
            _streamAccessorComms.Write(_dataCommunication, 0, writeSize);

            WaitHandle.SignalAndWait(_informationReadyForProfiler, _informationReadByProfiler);
            _informationReadByProfiler.Reset();
        }
    }
}
