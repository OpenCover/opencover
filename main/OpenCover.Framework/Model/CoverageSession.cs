using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    public class CoverageSession
    {
        public string SessionId { get; set; }
        public Module[] Modules { get; set; }
    }
}
