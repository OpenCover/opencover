using System;
using System.Diagnostics;
using System.ServiceModel;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunication : IProfilerCommunication
    {
        private readonly IFilter _filter;

        public ProfilerCommunication()
        {
            _filter = new Filter();
            _filter.AddFilter("-[mscorlib]*");
            _filter.AddFilter("-[System.*]*");
            _filter.AddFilter("+[*]*");
        }

        public void Start()
        {
            Trace.WriteLine("->Start");
        } 

        public bool ShouldTrackAssembly(string assemblyName)
        {
            Trace.WriteLine(string.Format("->ShouldTrackAssembly({0})", assemblyName));
            return _filter.UseAssembly(assemblyName);
        }

        public void Stop()
        {
            Trace.WriteLine("->Stop");
        }
    }
}