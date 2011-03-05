using System;
using System.Diagnostics;
using System.ServiceModel;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunication : IProfilerCommunication
    {
        public void Start()
        {
            Trace.WriteLine("->Start");
        }

        public void Stop()
        {
            Trace.WriteLine("->Stop");
        }
    }
}