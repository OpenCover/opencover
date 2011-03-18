using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity;
using OpenCover.Framework;
using OpenCover.Framework.Service;

namespace OpenCover.Console
{
    class Program
    {
        /// <summary>
        /// This is the initial console harness - it may become the full thing
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var parser = new CommandLineParser(string.Join("", args));

            var container = new Bootstrapper();
            var filter = new Filter();
            filter.AddFilter("-[mscorlib]*");
            filter.AddFilter("-[System.*]*");
            filter.AddFilter("+[*]*");
            container.Initialise(filter, parser);

            try
            {
                parser.ExtractAndValidateArguments();

                if (parser.PrintUsage)
                {
                    System.Console.WriteLine(parser.Usage());
                    return;
                }

                if (parser.HostOnly)
                {
                    
                    var host = new ProfilerServiceHost(container.Container);
                    host.Open(parser.PortNumber);
                    Thread.Sleep(new TimeSpan(0, 0, 0, parser.HostOnlySeconds));
                    host.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Incorrect Arguments: {0}", ex.Message);
                System.Console.WriteLine(parser.Usage());
                return;
            }

            try
            {
                if (parser.Register) ProfilerRegistration.Register(parser.UserRegistration);

                var host = new ProfilerServiceHost(container.Container);
                host.Open(parser.PortNumber);

                if (Directory.Exists(parser.TargetDir)) Environment.CurrentDirectory = parser.TargetDir;

                var startInfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, parser.Target));
                startInfo.EnvironmentVariables.Add("Cor_Profiler", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
                startInfo.EnvironmentVariables.Add("Cor_Enable_Profiling", "1");
                startInfo.EnvironmentVariables.Add("OpenCover_Port", parser.PortNumber.ToString());
                startInfo.Arguments = parser.TargetArgs;
                startInfo.UseShellExecute = false;

                var process = Process.Start(startInfo);

                process.WaitForExit();

                host.Close();

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
