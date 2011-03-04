using System;
using System.Diagnostics;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunication : IProfilerCommunication
    {
        public bool Start()
        {
            Trace.WriteLine("->Start");
            return true;
        }

        public bool Stop()
        {
            Trace.WriteLine("->Stop");
            return true;
        }
    }
}