using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    public class CoverageSession
    {
        public CoverageSession()
        {
            VisitPoints = new List<VisitPoint>();
        }

        public string SessionId { get; set; }
        public Module[] Modules { get; set; }
        public List<VisitPoint> VisitPoints { get; set; }

    }
}
