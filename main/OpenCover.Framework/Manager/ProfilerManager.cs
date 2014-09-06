//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCover.Framework.Communication;
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
        const int MaxMsgSize = 65536;
        private const int NumHandlesPerBlock = 32;

        private readonly ICommunicationManager _communicationManager;
        private readonly IPersistance _persistance;
        private readonly IMemoryManager _memoryManager;
        private readonly ICommandLine _commandLine;
        private readonly IPerfCounters _perfCounters;
        private MemoryManager.ManagedCommunicationBlock _mcb;

        private ConcurrentQueue<byte[]> _messageQueue;

        /// <summary>
        /// Create an instance of the profiler manager
        /// </summary>
        /// <param name="communicationManager"></param>
        /// <param name="persistance"></param>
        /// <param name="memoryManager"></param>
        /// <param name="commandLine"></param>
        /// <param name="perfCounters"></param>
        public ProfilerManager(ICommunicationManager communicationManager, IPersistance persistance, 
            IMemoryManager memoryManager, ICommandLine commandLine, IPerfCounters perfCounters)
        {
            _communicationManager = communicationManager;
            _persistance = persistance;
            _memoryManager = memoryManager;
            _commandLine = commandLine;
            _perfCounters = perfCounters;
        }

        public void RunProcess(Action<Action<StringDictionary>> process, string[] servicePrincipal)
        {
            var key = Guid.NewGuid().GetHashCode().ToString("X");
            var processMgmt = new AutoResetEvent(false);
            var queueMgmt = new AutoResetEvent(false);
            var environmentKeyRead = new AutoResetEvent(false);
            var handles = new List<WaitHandle> { processMgmt };

            string @namespace = servicePrincipal.Any() ? "Global" : "Local";

            _memoryManager.Initialise(@namespace, key, servicePrincipal);

            _messageQueue = new ConcurrentQueue<byte[]>();

            using (_mcb = new MemoryManager.ManagedCommunicationBlock(@namespace, key, MaxMsgSize, -1, servicePrincipal))
            {
                handles.Add(_mcb.ProfilerRequestsInformation);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        process(dictionary =>
                        {
                            if (dictionary == null) return;
                            dictionary[@"OpenCover_Profiler_Key"] = key;
                            dictionary[@"OpenCover_Profiler_Namespace"] = @namespace;
                            dictionary[@"OpenCover_Profiler_Threshold"] = _commandLine.Threshold.ToString(CultureInfo.InvariantCulture);

                            if (_commandLine.TraceByTest)
                                dictionary[@"OpenCover_Profiler_TraceByTest"] = "1";

                            dictionary["Cor_Profiler"] = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
                            dictionary["Cor_Enable_Profiling"] = "1";
                            dictionary["CoreClr_Profiler"] = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
                            dictionary["CoreClr_Enable_Profiling"] = "1";

                            if (_commandLine.Registration == Registration.Path32)
                                dictionary["Cor_Profiler_Path"] = ProfilerRegistration.GetProfilerPath(false);
                            if (_commandLine.Registration == Registration.Path64)
                                dictionary["Cor_Profiler_Path"] = ProfilerRegistration.GetProfilerPath(true);

                            environmentKeyRead.Set();
                        });
                    }
                    finally
                    {
                        processMgmt.Set();
                    }
                });

                ThreadPool.QueueUserWorkItem(state =>
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
                    ProcessMessages(handles.ToArray());
                    queueMgmt.WaitOne();
                }
            }
        }

        private bool _continueWait = true;

        private void ProcessMessages(WaitHandle[] handles)
        {
            var threadHandles = new List<Tuple<ManualResetEvent, ManualResetEvent>>();
            do
            {
                switch (WaitHandle.WaitAny(handles))
                {
                    case 0:
                        _continueWait = false;
                        break;

                    case 1:
                        _communicationManager.HandleCommunicationBlock(_mcb, (mcb, mmb) => threadHandles.Add(StartProcessingThread(mcb, mmb)));
                        break;
                }
            } while (_continueWait);

            foreach (var block in _memoryManager.GetBlocks.Select(b => b.Item2))
            {
                var data = new byte[block.BufferSize];
                block.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                block.StreamAccessorResults.Read(data, 0, block.BufferSize);
                _messageQueue.Enqueue(data);    
            }

            if (threadHandles.Any())
            {
                var tasks = threadHandles
                    .Select((e, index) => new {Pair = e, Block = index / NumHandlesPerBlock})
                    .GroupBy(g => g.Block)
                    .Select(g => g.Select(a => a.Pair))
                    .Select(g => Task.Factory.StartNew(() =>
                    {
                        g.Select(h => h.Item1).ToList().ForEach(h => h.Set());
                        WaitHandle.WaitAll(g.Select(h => h.Item2).Cast<WaitHandle>().ToArray(), new TimeSpan(0, 0, 20));
                    })).ToArray();
                Task.WaitAll(tasks);
            }

            _messageQueue.Enqueue(new byte[0]);
        }

        private Tuple<ManualResetEvent, ManualResetEvent> StartProcessingThread(IManagedCommunicationBlock communicationBlock, IManagedMemoryBlock memoryBlock)
        {
            var threadActivated = new AutoResetEvent(false);
            var terminateThread = new ManualResetEvent(false);
            var threadTerminated = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(state =>
            {
                var processEvents = new WaitHandle[]
                                    {
                                        communicationBlock.ProfilerRequestsInformation,
                                        memoryBlock.ProfilerHasResults,
                                        terminateThread
                                    };
                threadActivated.Set();
                while(true)
                {
                    switch (WaitHandle.WaitAny(processEvents))
                    {
                        case 0:
                            _communicationManager.HandleCommunicationBlock(communicationBlock, (cB, mB) => { });
                            break;

                        case 1:
                            {
                                var data = _communicationManager.HandleMemoryBlock(memoryBlock);
                                _messageQueue.Enqueue(data);
                            }
                            break;

                        case 2:
                            threadTerminated.Set();
                            return;

                    }
                }
            });
            threadActivated.WaitOne();
            return new Tuple<ManualResetEvent, ManualResetEvent>(terminateThread, threadTerminated);
        }
    }
}
