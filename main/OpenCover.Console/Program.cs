//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Principal;
using System.ServiceProcess;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Utility;
using log4net;
using log4net.Core;

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
            var returnCodeOffset = 0;
            var logger = LogManager.GetLogger(typeof (Bootstrapper));
            try
            {
                CommandLineParser parser;
                if (!ParseCommandLine(args, out parser)) return parser.ReturnCodeOffset + 1;

                LogManager.GetRepository().Threshold = parser.LogLevel;

                returnCodeOffset = parser.ReturnCodeOffset;
                var filter = BuildFilter(parser);

                string outputFile;
                if (!GetFullOutputFile(parser, out outputFile)) return returnCodeOffset + 1;

                IPerfCounters perfCounter = new NullPerfCounter();
                if (parser.EnablePerformanceCounters)
                {
                    if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        perfCounter = new PerfCounters();
                    }
                    else
                    {
                        throw  new InvalidCredentialException("You must be running as an Administrator to enable performance counters.");
                    }
                }

                using (var container = new Bootstrapper(logger))
                {
                    var persistance = new FilePersistance(parser, logger);
                    container.Initialise(filter, parser, persistance, perfCounter);
                    persistance.Initialise(outputFile);
                    var registered = false;

                    try
                    {
                        if (parser.Register)
                        {
                            ProfilerRegistration.Register(parser.Registration);
                            registered = true;
                        }

                        var harness = container.Resolve<IProfilerManager>();

                        var servicePrincipal =
                            (parser.Service
                                ? new[] { ServiceEnvironmentManagement.MachineQualifiedServiceAccountName(parser.Target) }
                                : new string[0]).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                        harness.RunProcess((environment) =>
                                               {
                                                   returnCode = 0;
                                                   if (parser.Service)
                                                   {
                                                       RunService(parser, environment, logger);
                                                   }
                                                   else
                                                   {
                                                       returnCode = RunProcess(parser, environment);
                                                   }
                                               }, servicePrincipal);

                        DisplayResults(persistance, parser, logger);

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Exception: {0}\n{1}", ex.Message, ex.InnerException));
                        throw;
                    }
                    finally
                    {
                        if (parser.Register && registered)
                            ProfilerRegistration.Unregister(parser.Registration);
                    }
                }

                perfCounter.ResetCounters();
            }
            catch (Exception ex)
            {
                if (logger.IsFatalEnabled)
                {
                    logger.FatalFormat("An exception occured: {0}", ex.Message);
                    logger.FatalFormat("stack: {0}", ex.StackTrace);
                }

                returnCode = returnCodeOffset + 1;
            }

            return returnCode;
        }

        private static void RunService(CommandLineParser parser, Action<StringDictionary> environment, ILog logger)
        {
            if (ServiceEnvironmentManagementEx.IsServiceDisabled(parser.Target))
            {
                logger.ErrorFormat("The service '{0}' is disabled. Please enable the service.",
                    parser.Target);
                return;
            }

            var service = new ServiceController(parser.Target);
            try
            {

            if (service.Status != ServiceControllerStatus.Stopped)
            {
                    logger.ErrorFormat(
                        "The service '{0}' is already running. The profiler cannot attach to an already running service.",
                    parser.Target);
                return;
            }

            // now to set the environment variables
            var profilerEnvironment = new StringDictionary();
            environment(profilerEnvironment);

            var serviceEnvironment = new ServiceEnvironmentManagement();

            try
            {
                serviceEnvironment.PrepareServiceEnvironment(
                    parser.Target,
                        parser.ServiceEnvironment,
                    (from string key in profilerEnvironment.Keys
                        select string.Format("{0}={1}", key, profilerEnvironment[key])).ToArray());

                // now start the service
                    var old = service;
                service = new ServiceController(parser.Target);
                    old.Dispose();
                service.Start();
                logger.InfoFormat("Service starting '{0}'", parser.Target);
                service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                logger.InfoFormat("Service started '{0}'", parser.Target);
            }
            catch (InvalidOperationException fault)
            {
                logger.FatalFormat("Service launch failed with '{0}'", fault);
            }
            finally 
            {
                // once the serice has started set the environment variables back - just in case
                serviceEnvironment.ResetServiceEnvironment();
            }

            // and wait for it to stop
            service.WaitForStatus(ServiceControllerStatus.Stopped);
            logger.InfoFormat("Service stopped '{0}'", parser.Target);
        }
            finally
            {
                service.Dispose();
            }
        }

        private static IEnumerable<string> GetSearchPaths(string targetDir)
        {
            return (new[] { Environment.CurrentDirectory, targetDir }).Concat((Environment.GetEnvironmentVariable("PATH") ?? Environment.CurrentDirectory).Split(Path.PathSeparator));            
        } 

        private static string ResolveTargetPathname(CommandLineParser parser)
        {
            var expandedTargetName = Environment.ExpandEnvironmentVariables(parser.Target);
            var expandedTargetDir = Environment.ExpandEnvironmentVariables(parser.TargetDir ?? string.Empty);
            return Path.IsPathRooted(expandedTargetName) ? Path.Combine(Environment.CurrentDirectory, expandedTargetName) :
                    GetSearchPaths(expandedTargetDir).Select(dir => Path.Combine(dir.Trim('"'), expandedTargetName)).FirstOrDefault(File.Exists) ?? expandedTargetName;
        }
        
        private static int RunProcess(CommandLineParser parser, Action<StringDictionary> environment)
        {
            var returnCode = 0;

            var targetPathname = ResolveTargetPathname(parser);
            System.Console.WriteLine("Executing: {0}", Path.GetFullPath(targetPathname));

            var startInfo = new ProcessStartInfo(targetPathname);
            environment(startInfo.EnvironmentVariables);

            if (parser.OldStyleInstrumentation)
                startInfo.EnvironmentVariables[@"OpenCover_Profiler_Instrumentation"] = "oldSchool";

            startInfo.Arguments = parser.TargetArgs;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = parser.TargetDir;

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (parser.ReturnTargetCode)
                returnCode = process.ExitCode;
            return returnCode;
        }

        private static void DisplayResults(IPersistance persistance, ICommandLine parser, ILog logger)
        {
            if (!logger.IsInfoEnabled) return;
 
            var CoverageSession = persistance.CoverageSession;

            var totalClasses = 0;
            var visitedClasses = 0;

            var altTotalClasses = 0;
            var altVisitedClasses = 0;

            var totalMethods = 0;
            var visitedMethods = 0;

            var altTotalMethods = 0;
            var altVisitedMethods = 0;

            var unvisitedClasses = new List<string>();
            var unvisitedMethods = new List<string>();

            if (CoverageSession.Modules != null)
            {
                foreach (var @class in
                    from module in CoverageSession.Modules.Where(x=>x.Classes != null)
                    from @class in module.Classes.Where(c => !c.ShouldSerializeSkippedDueTo())
                    select @class)
                {
                    if (@class.Methods == null) continue;

                    if ((@class.Methods.Any(x => !x.ShouldSerializeSkippedDueTo() && x.SequencePoints.Any(y => y.VisitCount > 0))))
                    {
                        visitedClasses += 1;
                        totalClasses += 1;
                    }
                    else if ((@class.Methods.Any(x => x.FileRef != null)))
                    {
                        totalClasses += 1;
                        unvisitedClasses.Add(@class.FullName);
                    }

                    if (@class.Methods.Any(x => x.Visited))
                    {
                        altVisitedClasses += 1;
                        altTotalClasses += 1;
                    }
                    else if (@class.Methods.Any())
                    {
                        altTotalClasses += 1;
                    }

                    foreach (var method in @class.Methods.Where(x=> !x.ShouldSerializeSkippedDueTo()))
                    {
                        if ((method.SequencePoints.Any(x => x.VisitCount > 0)))
                        {
                            visitedMethods += 1;
                            totalMethods += 1;
                        }
                        else if (method.FileRef != null)
                        {
                            totalMethods += 1;
                            unvisitedMethods.Add(string.Format("{0}", method.Name));
                        }

                        altTotalMethods += 1;
                        if (method.Visited)
                        {
                            altVisitedMethods += 1;
                        }
                    }
                }
            }

            if (totalClasses > 0)
            {           
                logger.InfoFormat("Visited Classes {0} of {1} ({2})", visitedClasses,
                                  totalClasses, Math.Round(visitedClasses * 100.0 / totalClasses, 2));
                logger.InfoFormat("Visited Methods {0} of {1} ({2})", visitedMethods,
                                  totalMethods, Math.Round(visitedMethods * 100.0 / totalMethods, 2));
                logger.InfoFormat("Visited Points {0} of {1} ({2})", CoverageSession.Summary.VisitedSequencePoints,
                                  CoverageSession.Summary.NumSequencePoints, CoverageSession.Summary.SequenceCoverage);
                logger.InfoFormat("Visited Branches {0} of {1} ({2})", CoverageSession.Summary.VisitedBranchPoints,
                                  CoverageSession.Summary.NumBranchPoints, CoverageSession.Summary.BranchCoverage);

                logger.InfoFormat("");
                logger.InfoFormat(
                    "==== Alternative Results (includes all methods including those without corresponding source) ====");
                logger.InfoFormat("Alternative Visited Classes {0} of {1} ({2})", altVisitedClasses,
                                  altTotalClasses, Math.Round(altVisitedClasses * 100.0 / altTotalClasses, 2));
                logger.InfoFormat("Alternative Visited Methods {0} of {1} ({2})", altVisitedMethods,
                                  altTotalMethods, Math.Round(altVisitedMethods * 100.0 / altTotalMethods, 2));

                if (parser.ShowUnvisited)
                {
                    logger.InfoFormat("");
                    logger.InfoFormat("====Unvisited Classes====");
                    foreach (var unvisitedClass in unvisitedClasses)
                    {
                        logger.InfoFormat(unvisitedClass);
                    }

                    logger.InfoFormat("");
                    logger.InfoFormat("====Unvisited Methods====");
                    foreach (var unvisitedMethod in unvisitedMethods)
                    {
                        logger.InfoFormat(unvisitedMethod);
                    }
                }
            }
            else
            {
                logger.InfoFormat("No results, this could be for a number of reasons. The most common reasons are:");
                logger.InfoFormat("    1) missing PDBs for the assemblies that match the filter please review the");
                logger.InfoFormat("    output file and refer to the Usage guide (Usage.rtf) about filters.");
                logger.InfoFormat("    2) the profiler may not be registered correctly, please refer to the Usage");
                logger.InfoFormat("    guide and the -register switch.");
            }
        }

        private static bool GetFullOutputFile(CommandLineParser parser, out string outputFile)
        {
            outputFile = Path.Combine(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables(parser.OutputFile));
            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                System.Console.WriteLine("Output folder does not exist; please create it and make sure appropriate permissions are set.");
                return false;
            }
            return true;
        }

        private static IFilter BuildFilter(CommandLineParser parser)
        {
            var filter = new Filter();

            // apply filters
            if (!parser.NoDefaultFilters)
            {
                filter.AddFilter("-[mscorlib]*");
                filter.AddFilter("-[mscorlib.*]*");
                filter.AddFilter("-[System]*");
                filter.AddFilter("-[System.*]*");
                filter.AddFilter("-[Microsoft.VisualBasic]*");
            }


            if (parser.Filters.Count == 0 && string.IsNullOrEmpty(parser.FilterFile))
            {
                filter.AddFilter("+[*]*");
            }
            else
            {
                if (!string.IsNullOrEmpty(parser.FilterFile))
                {
                    if (!File.Exists(parser.FilterFile))
                        System.Console.WriteLine("FilterFile '{0}' cannot be found - have you specified your arguments correctly?", parser.FilterFile);
                    else
                    {
                        var filters = File.ReadAllLines(parser.FilterFile);
                        filters.ToList().ForEach(filter.AddFilter);
                    }
                }
                if (parser.Filters.Count > 0)
                {
                    parser.Filters.ForEach(filter.AddFilter);
                }
            }

            filter.AddAttributeExclusionFilters(parser.AttributeExclusionFilters.ToArray());
            filter.AddFileExclusionFilters(parser.FileExclusionFilters.ToArray());
            filter.AddTestFileFilters(parser.TestFilters.ToArray());

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
                    "An error occurred whilst parsing the command line; try -? for command line arguments.");
            }

            try
            {
                parser.ExtractAndValidateArguments();

                if (parser.PrintUsage)
                {
                    System.Console.WriteLine(parser.Usage());
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(parser.TargetDir) && !Directory.Exists(parser.TargetDir))
                {
                    System.Console.WriteLine("TargetDir '{0}' cannot be found - have you specified your arguments correctly?", parser.TargetDir);
                    return false;
                }

                if (parser.Service)
                {
                    try
                    {
                        using (var service = new ServiceController(parser.Target))
                        {
                        var name = service.DisplayName;
                    }
                    }
                    catch (Exception)
                    {
                        System.Console.WriteLine("Service '{0}' cannot be found - have you specified your arguments correctly?", parser.Target);
                        return false;
                    }                    
                }
                else if (!File.Exists(ResolveTargetPathname(parser)))
                {
                    System.Console.WriteLine("Target '{0}' cannot be found - have you specified your arguments correctly?", parser.Target);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Incorrect Arguments: {0}", ex.Message);
                System.Console.WriteLine(parser.Usage());
                return false;
            }
            return true;
        }
    }
}
