using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using OpenCover.Framework;
using OpenCover.Framework.Service;

namespace OpenCover.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            var parser = new CommandLineParser(string.Join("", args));

            try
            {

                if (parser.Register) ProfilerRegistration.Register(parser.UserRegistration);

                var baseAddress = new Uri(string.Format("http://localhost:{0}/OpenCover.Profiler", parser.PortNumber));
                var selfHost = new ServiceHost(typeof(ProfilerCommunication), baseAddress);

                ServiceMetadataBehavior smb =
                selfHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (smb == null)
                    smb = new ServiceMetadataBehavior();

                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy12;
                selfHost.Description.Behaviors.Add(smb);

                selfHost.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "mex");

                var binding = new WSHttpBinding();
                binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                binding.Security.Mode = SecurityMode.None;

                selfHost.AddServiceEndpoint(
                    typeof(IProfilerCommunication), binding, baseAddress);

                selfHost.Open();

                var startInfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "OpenCover.Simple.Target.exe"));
                startInfo.EnvironmentVariables.Add("Cor_Profiler", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
                startInfo.EnvironmentVariables.Add("Cor_Enable_Profiling", "1");
                startInfo.EnvironmentVariables.Add("OpenCover_Port", parser.PortNumber.ToString());
                startInfo.UseShellExecute = false;

                var process = Process.Start(startInfo);

                process.WaitForExit();

                selfHost.Close();

            }
            catch (CommunicationException ce)
            {
                Debug.WriteLine("CommunicationException: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
            }
            finally
            {
                if (parser.Register) ProfilerRegistration.Unregister(parser.UserRegistration);
            }
        }
    }
}
