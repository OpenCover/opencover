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
using OpenCover.Framework.Utility;

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

        private readonly ICommunicationManager _communicationManager;
        private readonly IPersistance _persistance;
        private readonly IMemoryManager _memoryManager;
        private readonly ICommandLine _commandLine;
        private readonly IPerfCounters _perfCounters;
        private MemoryManager.ManagedCommunicationBlock _mcb;

        private ConcurrentQueue<byte[]> _messageQueue;

        public ProfilerManager(ICommunicationManager communicationManager, IPersistance persistance, 
            IMemoryManager memoryManager, ICommandLine commandLine, IPerfCounters perfCounters)
        {
            _communicationManager = communicationManager;
            _persistance = persistance;
            _memoryManager = memoryManager;
            _commandLine = commandLine;
            _perfCounters = perfCounters;
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

            _messageQueue = new ConcurrentQueue<byte[]>();

            using (_mcb = new MemoryManager.ManagedCommunicationBlock(@namespace, key, maxMsgSize, -1))
            {
                handles.Add(_mcb.ProfilerRequestsInformation);

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
                        //// use this block to introduce a delay in to the queue processing
                        //if (_messageQueue.Count < 100)
                        //  Thread.Sleep(10);

                        byte[] data;
                        while (!_messageQueue.TryDequeue(out data))
                            Thread.Yield();

                        _perfCounters.CurrentMemoryQueueSize = _messageQueue.Count;
                        _perfCounters.IncrementBlocksReceived();

                        if (data.Length == 0)
                        {
                            _communicationManager.Complete();
                            queueMgmt.Set();
                            return;
                        }
                        _persistance.SaveVisitData(data);
                    }
                });

                // wait for the environment key to be read
                if (WaitHandle.WaitAny(new WaitHandle[] {environmentKeyRead}, new TimeSpan(0, 0, 0, 10)) != -1)
                {
                    ProcessMessages(handles);
                    queueMgmt.WaitOne();
                }
            }
        }

        private void ProcessMessages(List<WaitHandle> handles)
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
                        _communicationManager.HandleCommunicationBlock(_mcb);
                        break;

                    default:
                        var block = _memoryManager.GetBlocks[@case - 2];
                        var data = _communicationManager.HandleMemoryBlock(block);
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
    }
}
