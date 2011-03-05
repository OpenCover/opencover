using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace OpenCover.Framework.Service
{
    public class ProfilerServiceHost
    {
        private ServiceHost _serviceHost;
        public void Open(int port)
        {
            var baseAddress = new Uri(string.Format("http://localhost:{0}/OpenCover.Profiler", port));
            _serviceHost = new ServiceHost(typeof(ProfilerCommunication), baseAddress);

            var smb = _serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();

            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy12;
            _serviceHost.Description.Behaviors.Add(smb);

            _serviceHost.AddServiceEndpoint(
                ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexHttpBinding(),
                "mex");

            var binding = new WSHttpBinding
            {
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                Security = { Mode = SecurityMode.None }
            };

            _serviceHost.AddServiceEndpoint(
                typeof(IProfilerCommunication), binding, baseAddress);

            _serviceHost.AddServiceEndpoint(typeof(IProfilerCommunication),
                new NetTcpBinding(),
                new Uri(string.Format("net.tcp://localhost:{0}/Service", port + 1)));

            _serviceHost.Open();
        }

        public void Close()
        {
            _serviceHost.Close();
        }
    }
}
