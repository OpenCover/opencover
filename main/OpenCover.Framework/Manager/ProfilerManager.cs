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
using log4net;
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
        private const string ProfilerGuid = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";

        private readonly ICommunicationManager _communicationManager;
        private readonly IPersistance _persistance;
        private readonly IMemoryManager _memoryManager;
        private readonly ICommandLine _commandLine;
        private readonly IPerfCounters _perfCounters;
        private MemoryManager.ManagedCommunicationBlock _mcb;

        private ConcurrentQueue<byte[]> _messageQueue;

        private static readonly ILog DebugLogger = LogManager.GetLogger("DebugLogger");

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

        /// <summary>
        /// Start the target process
        /// </summary>
        /// <param name="process"></param>
        /// <param name="servicePrincipal"></param>
        public void RunProcess(Action<Action<StringDictionary>> process, string[] servicePrincipal)
        {
            var key = Guid.NewGuid().GetHashCode().ToString("X");
            string @namespace = servicePrincipal.Any() ? "Global" : "Local";

            _memoryManager.Initialise(@namespace, key, servicePrincipal);
            _messageQueue = new ConcurrentQueue<byte[]>();

            using (_mcb = new MemoryManager.ManagedCommunicationBlock(@namespace, key, MaxMsgSize, -1, servicePrincipal))
            using (var processMgmt = new AutoResetEvent(false))
            using (var queueMgmt = new AutoResetEvent(false))
            using (var environmentKeyRead = new AutoResetEvent(false))
            {
                var handles = new List<WaitHandle> { processMgmt, _mcb.ProfilerRequestsInformation };

                ThreadPool.QueueUserWorkItem(
                    SetProfilerAttributes(process, key, @namespace, environmentKeyRead, processMgmt));
                ThreadPool.QueueUserWorkItem(SaveVisitData(queueMgmt));

                // wait for the environment key to be read
                if (WaitHandle.WaitAny(new WaitHandle[] {environmentKeyRead}, new TimeSpan(0, 0, 0, 10)) != -1)
                {
                    ProcessMessages(handles.ToArray());
                    queueMgmt.WaitOne();
                }
            }
        }

        private WaitCallback SetProfilerAttributes(Action<Action<StringDictionary>> process, string profilerKey, 
            string profilerNamespace, EventWaitHandle environmentKeyRead, EventWaitHandle processMgmt)
        {
            return state =>
            {
                try
                {
                    process(dictionary =>
                    {
                        if (dictionary == null) return;
                        SetProfilerAttributesOnDictionary(profilerKey, profilerNamespace, dictionary);

                        environmentKeyRead.Set();
                    });
                }
                finally
                {
                    processMgmt.Set();
                }
            };
        }

        private void SetProfilerAttributesOnDictionary(string profilerKey, string profilerNamespace, StringDictionary dictionary)
        {
            dictionary[@"OpenCover_Profiler_Key"] = profilerKey;
            dictionary[@"OpenCover_Profiler_Namespace"] = profilerNamespace;
            dictionary[@"OpenCover_Profiler_Threshold"] = _commandLine.Threshold.ToString(CultureInfo.InvariantCulture);

            if (_commandLine.TraceByTest)
                dictionary[@"OpenCover_Profiler_TraceByTest"] = "1";

            dictionary["Cor_Profiler"] = ProfilerGuid;
            dictionary["Cor_Enable_Profiling"] = "1";
            dictionary["CoreClr_Profiler"] = ProfilerGuid;
            dictionary["CoreClr_Enable_Profiling"] = "1";
           
            switch (_commandLine.Registration)
            {
                case Registration.Path32:
                    string profilerPath32 = ProfilerRegistration.GetProfilerPath(false);
                    dictionary["Cor_Profiler_Path"] = profilerPath32;
                    dictionary["CorClr_Profiler_Path"] = profilerPath32;
                    break;
                case Registration.Path64:
                    string profilerPath64 = ProfilerRegistration.GetProfilerPath(true);
                    dictionary["Cor_Profiler_Path"] = profilerPath64;
                    dictionary["CorClr_Profiler_Path"] = profilerPath64;
                    break;
            }
        }

        private WaitCallback SaveVisitData(EventWaitHandle queueMgmt)
        {
            return state =>
            {
                while (true)
                {
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
            };
        }

        /// <summary>
        /// wait for how long
        /// </summary>
        internal static int BufferWaitCount = 30;

        private bool _continueWait = true;

        private void ProcessMessages(WaitHandle[] handles)
        {
            var threadHandles = new List<Tuple<EventWaitHandle, EventWaitHandle>>();
            do
            {
                switch (WaitHandle.WaitAny(handles))
                {
                    case 0:
                        _continueWait = false;
                        break;

                    case 1:
                        _communicationManager.HandleCommunicationBlock(_mcb,
                            block => Task.Factory.StartNew(() =>
                            {
                                lock (threadHandles)
                                {
                                    threadHandles.Add(StartProcessingThread(block));
                                }
                            }));
                        break;
                }
            } while (_continueWait);

            // we need to let the profilers dump the thread buffers over before they close - max 15s (ish)
            var i = 0;
            while (i < BufferWaitCount && _memoryManager.GetBlocks.Any(b => b.Active))
            {
                DebugLogger.InfoFormat("Waiting for {0} processes to close", _memoryManager.GetBlocks.Count(b => b.Active));
                Thread.Sleep(500);
                i++;
            }

            // grab anything left in the main buffers
            foreach (var block in _memoryManager.GetBlocks.Where(b => b.Active).Select(b => b.MemoryBlock))
            {
                var data = new byte[block.BufferSize];
                block.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
                block.StreamAccessorResults.Read(data, 0, block.BufferSize);
                _messageQueue.Enqueue(data);
            }

            lock (threadHandles)
            {
                if (threadHandles.Any())
                {
                    var tasks = threadHandles
                        .Select((e, index) => new {Pair = e, Block = index/NumHandlesPerBlock})
                        .GroupBy(g => g.Block)
                        .Select(g => g.Select(a => a.Pair).ToList())
                        .Select(g => Task.Factory.StartNew(() =>
                        {
                            g.Select(h => h.Item1).ToList().ForEach(h => h.Set());
                            WaitHandle.WaitAll(g.Select(h => h.Item2).ToArray<WaitHandle>(), new TimeSpan(0, 0, 20));
                        })).ToArray();
                    Task.WaitAll(tasks);

                    foreach (var threadHandle in threadHandles)
                    {
                        threadHandle.Item1.Dispose();
                        threadHandle.Item2.Dispose();
                    }
                    threadHandles.Clear();
                }
            }

            _messageQueue.Enqueue(new byte[0]);
        }

        private Tuple<EventWaitHandle, EventWaitHandle> StartProcessingThread(ManagedBufferBlock block)
        {
            DebugLogger.InfoFormat("Starting Process Block => {0}", block.BufferId);
            var terminateThread = new ManualResetEvent(false);
            var threadTerminated = new ManualResetEvent(false);

            using (var threadActivated = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(ProcessBlock(block, terminateThread,
                    threadActivated, threadTerminated));
                threadActivated.WaitOne();
            }
            DebugLogger.InfoFormat("Started Process Block => {0}", block.BufferId);
            return new Tuple<EventWaitHandle, EventWaitHandle>(terminateThread, threadTerminated);
        }

        private WaitCallback ProcessBlock(ManagedBufferBlock block,
            WaitHandle terminateThread, EventWaitHandle threadActivated, EventWaitHandle threadTerminated)
        {
            return state =>
            {
                var processEvents = new []
                {
                    block.CommunicationBlock.ProfilerRequestsInformation,
                    block.MemoryBlock.ProfilerHasResults,
                    terminateThread
                };
                threadActivated.Set();
                
                while(block.Active)
                {
                    switch (WaitHandle.WaitAny(processEvents))
                    {
                        case 0:
                            _communicationManager.HandleCommunicationBlock(block.CommunicationBlock, b => { });
                            break;
                        case 1:
                            var data = _communicationManager.HandleMemoryBlock(block.MemoryBlock);
                            // don't let the queue get too big as using too much memory causes 
                            // problems i.e. the target process closes down but the host takes 
                            // ages to shutdown; this is a compromise. 
                            _messageQueue.Enqueue(data);
                            if (_messageQueue.Count > 400)
                            {
                                do
                                {
                                    Thread.Yield();
                                } while (_messageQueue.Count > 200);
                            }
                            break;
                        case 2:
                            threadTerminated.Set();
                            return;
                    }
                }
                threadTerminated.Set();
                _memoryManager.RemoveDeactivatedBlocks();
            };
        }
    }
}
