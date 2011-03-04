using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenCover.Framework;

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

                var startInfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "OpenCover.Simple.Target.exe"));
                startInfo.EnvironmentVariables.Add("Cor_Profiler", "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
                startInfo.EnvironmentVariables.Add("Cor_Enable_Profiling", "1");
                startInfo.UseShellExecute = false;

                var process = Process.Start(startInfo);

                process.WaitForExit();

            }
            finally
            {
                if (parser.Register) ProfilerRegistration.Unregister(parser.UserRegistration);
            }
        }
    }
}
