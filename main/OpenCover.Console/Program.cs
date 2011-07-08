//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Diagnostics;
using System.IO;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;
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
            var parser = new CommandLineParser(args);

            var container = new Bootstrapper();
            var filter = new Filter();
            var persistance = new FilePersistance();
            
            container.Initialise(filter, parser, persistance);

            try
            {
                parser.ExtractAndValidateArguments();

                if (parser.PrintUsage)
                {
                    System.Console.WriteLine(parser.Usage());
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Incorrect Arguments: {0}", ex.Message);
                System.Console.WriteLine(parser.Usage());
                return;
            }

            // apply filters
            if (!parser.NoDefaultFilters)
            {
                filter.AddFilter("-[mscorlib]*");
                filter.AddFilter("-[System]*");
                filter.AddFilter("-[System.*]*");
                filter.AddFilter("-[Microsoft.VisualBasic]*");
            }

            if (parser.Filters.Count == 0)
            {
                filter.AddFilter("+[*]*");
            }
            else
            {
                parser.Filters.ForEach(filter.AddFilter);
            }

            persistance.Initialise(Path.Combine(Environment.CurrentDirectory, parser.OutputFile));
            bool registered = false;
            try
            {
                try
                {
                    if (parser.Register)
                    {
                        ProfilerRegistration.Register(parser.UserRegistration,
                                                      parser.Architecture == Architecture.Arch64);
                        registered = true;
                    }
                    var harness = (IProfilerManager) container.Container.Resolve(typeof (IProfilerManager), null);

                    harness.RunProcess((environment) =>
                                           {
                                               var startInfo =
                                                   new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory,
                                                                                     parser.Target));
                                               startInfo.EnvironmentVariables.Add("Cor_Profiler",
                                                                                  parser.Architecture ==
                                                                                  Architecture.Arch64
                                                                                      ? "{A7A1EDD8-D9A9-4D51-85EA-514A8C4A9100}"
                                                                                      : "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}");
                                               startInfo.EnvironmentVariables.Add("Cor_Enable_Profiling", "1");
                                               environment(startInfo.EnvironmentVariables);

                                               startInfo.Arguments = parser.TargetArgs;
                                               startInfo.UseShellExecute = false;
                                               startInfo.WorkingDirectory = parser.TargetDir;

                                               var process = Process.Start(startInfo);
                                               process.WaitForExit();
                                           });

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: {0}\n{1}", ex.Message, ex.InnerException);
                    throw;
                }
                finally
                {
                    if (parser.Register && registered)
                        ProfilerRegistration.Unregister(parser.UserRegistration, parser.Architecture == Architecture.Arch64);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("An exception occured: {0}", ex.Message);
            }

        }
    }
}
