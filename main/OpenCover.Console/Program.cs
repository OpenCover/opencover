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
        /// <returns></returns>
        static int Main(string[] args)
        {
            var returnCode = 0;
            try
            {
                CommandLineParser parser;
                if (ParseCommandLine(args, out parser)) return returnCode;

                var filter = BuildFilter(parser);

                string outputFile;
                if (GetFullOutputFile(parser, out outputFile)) return returnCode;

                var container = new Bootstrapper();
                var persistance = new FilePersistance(parser);

                container.Initialise(filter, parser, persistance);
                persistance.Initialise(outputFile);
                bool registered = false;

                try
                {
                    if (parser.Register)
                    {
                        ProfilerRegistration.Register(parser.UserRegistration);
                        registered = true;
                    }
                    var harness = (IProfilerManager)container.Container.Resolve(typeof(IProfilerManager), null);

                    harness.RunProcess((environment) =>
                                           {
                                               var startInfo =
                                                   new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory,
                                                                                     parser.Target));
                                               environment(startInfo.EnvironmentVariables);

                                               startInfo.Arguments = parser.TargetArgs;
                                               startInfo.UseShellExecute = false;
                                               startInfo.WorkingDirectory = parser.TargetDir;

                                               var process = Process.Start(startInfo);
                                               process.WaitForExit();

                                               if (parser.ReturnTargetCode)
                                                   returnCode = process.ExitCode;
                                           });

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Exception: {0}\n{1}", ex.Message, ex.InnerException));
                    throw;
                }
                finally
                {
                    if (parser.Register && registered)
                        ProfilerRegistration.Unregister(parser.UserRegistration);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("An exception occured: {0}", ex.Message);
                System.Console.WriteLine("stack: {0}", ex.StackTrace);
            }

            return returnCode;
        }

        private static bool GetFullOutputFile(CommandLineParser parser, out string outputFile)
        {
            outputFile = Path.Combine(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables(parser.OutputFile));
            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                System.Console.WriteLine(
                    "Output folder does not exist; please create it and make sure appropriate permissions are set.");
                return true;
            }
            return false;
        }

        private static Filter BuildFilter(CommandLineParser parser)
        {
            var filter = new Filter();

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
            return filter;
        }

        private static bool ParseCommandLine(string[] args, out CommandLineParser parser)
        {
            try
            {
                parser = new CommandLineParser(args);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    "An error occurred whilst parsing the command line; try /? for command line arguments.");
            }

            try
            {
                parser.ExtractAndValidateArguments();

                if (parser.PrintUsage)
                {
                    System.Console.WriteLine(parser.Usage());
                    return true;
                }

                if (!File.Exists(Environment.ExpandEnvironmentVariables(parser.Target)))
                {
                    System.Console.WriteLine("Target {0} cannot be found - have you specified your arguments correctly?");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Incorrect Arguments: {0}", ex.Message);
                System.Console.WriteLine(parser.Usage());
                return true;
            }
            return false;
        }
    }
}
