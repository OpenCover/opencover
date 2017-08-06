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
        private const int MaxMsgSize = 65536;
        private const int NumHandlesPerBlock = 32;
        private const string ProfilerGuid = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";

        private readonly ICommunicationManager _communicationManager;
        private readonly IPersistance _persistance;
        private readonly IMemoryManager _memoryManager;
        private readonly ICommandLine _commandLine;
        private readonly IPerfCounters _perfCounters;
        private MemoryManager.ManagedCommunicationBlock _mcb;

        private ConcurrentQueue<byte[]> _messageQueue;

        private readonly object _syncRoot = new object ();

        /// <summary>
        /// Syncronisation Root
        /// </summary>
        public object SyncRoot {
            get {
                return _syncRoot;
            }
        }

        /// <summary>
        /// wait for how long
        /// </summary>
        internal static int BufferWaitCount { get; set; }

        private static readonly ILog DebugLogger = LogManager.GetLogger("DebugLogger");

        private class ThreadTermination
        {
            public ThreadTermination()
            {
                // we do not dispose these events due to a race condition during shutdown
                CancelThreadEvent = new ManualResetEvent(false);
                ThreadFinishedEvent = new ManualResetEvent(false);
            }

            public ManualResetEvent CancelThreadEvent { get; private set; }
            public ManualResetEvent ThreadFinishedEvent { get; private set; }
        }

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

            using (_mcb = new MemoryManager.ManagedCommunicationBlock(@namespace, key, MaxMsgSize, -1, servicePrincipal)
                )
            using (var processMgmt = new AutoResetEvent(false))
            using (var queueMgmt = new AutoResetEvent(false))
            using (var environmentKeyRead = new AutoResetEvent(false))
            {
                var handles = new List<WaitHandle> {processMgmt, _mcb.ProfilerRequestsInformation};

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
                        if (dictionary != null)
                        {
                            SetProfilerAttributesOnDictionary(profilerKey, profilerNamespace, dictionary);
                            environmentKeyRead.Set();
                        }
                    });
                }
                finally
                {
                    processMgmt.Set();
                }
            };
        }

        private void SetProfilerAttributesOnDictionary(string profilerKey, string profilerNamespace,
            StringDictionary dictionary)
        {
            dictionary[@"OpenCover_Profiler_Key"] = profilerKey;
            dictionary[@"OpenCover_Profiler_Namespace"] = profilerNamespace;
            dictionary[@"OpenCover_Profiler_Threshold"] = _commandLine.Threshold.ToString(CultureInfo.InvariantCulture);

            if (_commandLine.TraceByTest)
                dictionary[@"OpenCover_Profiler_TraceByTest"] = "1";
            if (_commandLine.SafeMode)
                dictionary[@"OpenCover_Profiler_SafeMode"] = "1";

            dictionary["Cor_Profiler"] = ProfilerGuid;
            dictionary["Cor_Enable_Profiling"] = "1";
            dictionary["CoreClr_Profiler"] = ProfilerGuid;
            dictionary["CoreClr_Enable_Profiling"] = "1";
            dictionary["Cor_Profiler_Path"] = string.Empty;
            dictionary["CorClr_Profiler_Path"] = string.Empty;

            if (_commandLine.CommunicationTimeout > 0)
                dictionary["OpenCover_Profiler_ShortWait"] = _commandLine.CommunicationTimeout.ToString();

            var profilerPath = ProfilerRegistration.GetProfilerPath(_commandLine.Registration);
            if (profilerPath != null)
            {
                dictionary["Cor_Profiler_Path"] = profilerPath;
                dictionary["CorClr_Profiler_Path"] = profilerPath;
            }
            dictionary["OpenCover_SendVisitPointsTimerInterval"] = _commandLine.SendVisitPointsTimerInterval.ToString();
        }

        private WaitCallback SaveVisitData(EventWaitHandle queueMgmt)
        {
            return state =>
            {
                while (true)
                {
                    byte[] data;
                    while (!_messageQueue.TryDequeue(out data))
                        ThreadHelper.YieldOrSleep(100);

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

        private bool _continueWait = true;

        static ProfilerManager()
        {
            BufferWaitCount = 30;
        }

        private void ProcessMessages(WaitHandle[] handles)
        {
            var threadHandles = new List<ThreadTermination>();
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
                                lock (SyncRoot)
                                {
                                    threadHandles.Add(StartProcessingThread(block));
                                }
                            }));
                        break;
                    default:
                        break;
                }
            } while (_continueWait);

            _memoryManager.WaitForBlocksToClose(BufferWaitCount);

            _memoryManager.FetchRemainingBufferData(data => _messageQueue.Enqueue(data));

            lock (SyncRoot)
            {
                if (threadHandles.Any())
                {
                    var tasks = threadHandles
                        .Select((e, index) => new {ThreadTermination = e, Block = index/NumHandlesPerBlock})
                        .GroupBy(g => g.Block)
                        .Select(g => g.Select(a => a.ThreadTermination).ToList())
                        .Select(g => Task.Factory.StartNew(() =>
                        {
                            ConsumeException(() =>
                            {
                                g.Select(h => h.CancelThreadEvent).ToList().ForEach(h => h.Set());
                                WaitHandle.WaitAll(g.Select(h => h.ThreadFinishedEvent).ToArray<WaitHandle>(),
                                    new TimeSpan(0, 0, 20));
                            });
                        })).ToArray();

                    Task.WaitAll(tasks);

                    threadHandles.Clear();
                }
            }

            _messageQueue.Enqueue(new byte[0]);
        }

        // wrap exceptions when closing down
        private static void ConsumeException(Action doSomething)
        {
            try
            {
                doSomething();
            }
            catch (Exception ex)
            {
                DebugLogger.Error("An unexpected exception was encountered but consumed.", ex);
            }
        }

        private ThreadTermination StartProcessingThread(ManagedBufferBlock block)
        {
            DebugLogger.InfoFormat("Starting Process Block => {0}", block.BufferId);

            var threadTermination = new ThreadTermination();

            using (var threadActivatedEvent = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(ProcessBlock(block, threadActivatedEvent, threadTermination));
                threadActivatedEvent.WaitOne();
            }

            DebugLogger.InfoFormat("Started Process Block => {0}", block.BufferId);
            return threadTermination;
        }

        private WaitCallback ProcessBlock(ManagedBufferBlock block,
            EventWaitHandle threadActivatedEvent, ThreadTermination threadTermination)
        {
            return state =>
            {
                try
                {
                    var processEvents = new WaitHandle[]
                    {
                        block.CommunicationBlock.ProfilerRequestsInformation,
                        block.MemoryBlock.ProfilerHasResults,
                        threadTermination.CancelThreadEvent
                    };
                    threadActivatedEvent.Set();

                    try
                    {
                        if (ProcessActiveBlock(block, processEvents)) return;
                        _memoryManager.RemoveDeactivatedBlock(block);
                    }
                    finally
                    {
                        threadTermination.ThreadFinishedEvent.Set();
                    }
                }
                catch (ObjectDisposedException)
                {
                    /* an attempt to close thread has probably happened and the events disposed */
                }
            };
        }

        private bool ProcessActiveBlock(ManagedBufferBlock block, WaitHandle[] processEvents)
        {
            while (block.Active)
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
                                ThreadHelper.YieldOrSleep(100);
                            } while (_messageQueue.Count > 200);
                        }
                        break;
                    default: // 2
                        return true;
                }
            }
            return false;
        }
    }
}
