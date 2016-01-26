using System.Diagnostics;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// Expose some performance counters
    /// </summary>
    [ExcludeFromCoverage("Performance counters can only be created by Administrators")] 
    public class PerfCounters : IPerfCounters
    {
        private PerformanceCounter _memoryQueue;
        private PerformanceCounter _queueThroughput;

        /// <summary>
        /// get the current queue size
        /// </summary>
        public long CurrentMemoryQueueSize
        {
            get { return _memoryQueue.RawValue; }
            set { _memoryQueue.RawValue = value; }
        }

        /// <summary>
        /// Increment the block size
        /// </summary>
        public void IncrementBlocksReceived()
        {
            _queueThroughput.RawValue += 1;
        }

        /// <summary>
        /// Instantiate the Performance Counters
        /// </summary>
        public PerfCounters()
        {
            CreateCounters();
            ResetCounters();
        }

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
            _queueThroughput = new PerformanceCounter(CategoryName, QueueThroughput, false) { RawValue = 0 };
        }

        /// <summary>
        /// Reset all counters
        /// </summary>
        public void ResetCounters()
        {
            _memoryQueue.RawValue = 0;
            _queueThroughput.RawValue = 0;
        }
    }

    /// <summary>
    /// Used when the user is not running as an administrator and requested the perf option
    /// </summary>
    [ExcludeFromCoverage("Performance counters can only be created by Administrators")] 
    public class NullPerfCounter : IPerfCounters
    {
        /// <summary>
        /// A null performance counters implementation
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public long CurrentMemoryQueueSize { set; get; }

        /// <summary>
        /// Increment the number of blocks received
        /// </summary>
        public void IncrementBlocksReceived()
        {
            // null implementation
        }

        /// <summary>
        /// Reset all counters
        /// </summary>
        public void ResetCounters()
        {
            // null implementation
        }
    }
}