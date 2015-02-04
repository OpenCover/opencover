using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Target
{
    public class TimeWrapper
    {
        public DateTime CurrentTime { get { return DateTime.Now; } }
        public DateTime CurrentUtcTime { get { return DateTime.UtcNow; } }
    }
}
