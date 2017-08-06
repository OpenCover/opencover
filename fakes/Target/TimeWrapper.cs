using System;

namespace Target
{
    public class TimeWrapper
    {
        public DateTime CurrentTime { get { return DateTime.Now; } }
        public DateTime CurrentUtcTime { get { return DateTime.UtcNow; } }
    }
}
