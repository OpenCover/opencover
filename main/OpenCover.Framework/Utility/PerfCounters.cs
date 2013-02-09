using System.Diagnostics;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [ExcludeFromCoverage("Performance counters can only be created by Administrators")] 
    public class PerfCounters : IPerfCounters
    {
        private PerformanceCounter _memoryQueue;
        private PerformanceCounter _queueThrougput;

        public int CurrentMemoryQueueSize { set { _memoryQueue.RawValue = value; } }
        public void IncrementBlocksReceived()
        {
            _queueThrougput.RawValue += 1;
        }

        public PerfCounters()
        {
            CreateCounters();
            ResetCounters();
        }

        private const string InstanceName = "OpenCover";
        private const string CategoryName = "OpenCover";
        private const string MemoryQueue = "MemoryQueue";
        private const string QueueThroughput = "QueueThroughput";

        private void CreateCounters()
        {
            if (PerformanceCounterCategory.Exists(CategoryName))
                PerformanceCounterCategory.Delete(CategoryName);
            
            var counters = new CounterCreationDataCollection
                {
                    new CounterCreationData(MemoryQueue, "The number of memory blocks awaiting processing",
                                            PerformanceCounterType.NumberOfItems32),
                    new CounterCreationData(QueueThroughput, "The total number of memory blocks handed to the queue",
                                            PerformanceCounterType.NumberOfItems32)
                };
            PerformanceCounterCategory.Create(CategoryName, "OpenCover", 
                                              PerformanceCounterCategoryType.SingleInstance, counters);
            
            _memoryQueue = new PerformanceCounter(CategoryName, MemoryQueue, false) { RawValue = 0 };
            _queueThrougput = new PerformanceCounter(CategoryName, QueueThroughput, false) { RawValue = 0 };
        }

        public void ResetCounters()
        {
            _memoryQueue.RawValue = 0;
            _queueThrougput.RawValue = 0;
        }
    }

    /// <summary>
    /// Used when the user is not running as an administrator and requested the perf option
    /// </summary>
    [ExcludeFromCoverage("Performance counters can only be created by Administrators")] 
    public class NullPerfCounter : IPerfCounters
    {
        public int CurrentMemoryQueueSize { set; private get; }
        public void IncrementBlocksReceived()
        {
        }

        public void ResetCounters()
        {
        }
    }
}