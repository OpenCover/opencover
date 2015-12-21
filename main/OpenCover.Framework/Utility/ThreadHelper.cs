using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenCover.Framework.Utility
{
    internal static class ThreadHelper
    {
        public static void YieldOrSleep(int millisecondsInTimeout)
        {
            if (!Thread.Yield())
            {
                Thread.Sleep(millisecondsInTimeout);
            }
        }

        public static void YieldOrSleep(TimeSpan timespan)
        {
            if (!Thread.Yield())
            {
                Thread.Sleep(timespan);
            }
        }
    }
}
