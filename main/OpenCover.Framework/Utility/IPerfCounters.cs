using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPerfCounters
    {
        /// <summary>
        /// Report on the size of the memory queue 
        /// </summary>
        long CurrentMemoryQueueSize { get; set; }
        
        /// <summary>
        /// Increment the number of blocks received
        /// </summary>
        void IncrementBlocksReceived();

        /// <summary>
        /// Reset all counters
        /// </summary>
        void ResetCounters();
    }
}
