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
using System.Security.Principal;
using System.ServiceProcess;
using CrashReporterDotNET.com.drdump;
using OpenCover.Console.CrashReporter;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Utility;
using log4net;
using System.Management;
using OpenCover.Framework.Model;
using File = System.IO.File;

namespace OpenCover.Console
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("OpenCover");

        /// <summary>
        /// This is the initial console harness - it may become the full thing
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static int Main(string[] args)
        {
            int returnCode;
            var returnCodeOffset = 0;
           
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                if (!ParseCommandLine(args, out CommandLineParser parser)) 
                    return parser.ReturnCodeOffset + 1;

                LogManager.GetRepository().Threshold = parser.LogLevel;

                returnCodeOffset = parser.ReturnCodeOffset;
                var filter = BuildFilter(parser);
                var perfCounter = CreatePerformanceCounter(parser);

                if (!GetFullOutputFile(parser, out string outputFile)) 
                    return returnCodeOffset + 1;

                using (var container = new Bootstrapper(Logger))
                {
                    var persistance = new FilePersistance(parser, Logger);
                    container.Initialise(filter, parser, persistance, perfCounter);
                    if (!persistance.Initialise(outputFile, parser.MergeExistingOutputFile))
                        return returnCodeOffset + 1;

                    returnCode = RunWithContainer(parser, container, persistance);
                }

                perfCounter.ResetCounters();
            }
            catch (ExitApplicationWithoutReportingException)
            {
                Logger.ErrorFormat("If you are unable to resolve the issue please contact the OpenCover development team");
                Logger.ErrorFormat("see https://www.github.com/opencover/opencover/issues");
                returnCode = returnCodeOffset + 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal("At: Program.Main");
                Logger.FatalFormat("An {0} occurred: {1}", ex.GetType(), ex.Message);
                Logger.FatalFormat("stack: {0}", ex.StackTrace);
                Logger.FatalFormat("A report has been sent to the OpenCover development team.");
                Logger.ErrorFormat("If you are unable to resolve the issue please contact the OpenCover development team");
                Logger.ErrorFormat("see https://www.github.com/opencover/opencover/issues");

                ReportCrash(ex);

                returnCode = returnCodeOffset + 1;
            }

            return returnCode;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;
            //if (!(ex is ExitApplicationWithoutReportingException))
            {
                Logger.Fatal("At: CurrentDomainOnUnhandledException");
                Logger.FatalFormat("An {0} occurred: {1}", ex.GetType(), ex.Message);
                Logger.FatalFormat("stack: {0}", ex.StackTrace);
                Logger.FatalFormat("A report has been sent to the OpenCover development team...");

                ReportCrash((Exception)unhandledExceptionEventArgs.ExceptionObject);
            }

            Environment.Exit(0);
        }

        private static void ReportCrash(Exception exception)
        {
            try
            {
                using (var uploader = new HttpsCrashReporterReportUploader()) {

                    var state = new SendRequestState
                    {
                        AnonymousData = new AnonymousData
                        {
                            ApplicationGuid = new Guid("dbbb1d35-be49-45e2-b81d-84f1042c455d"),
                            Exception = exception,
                            ToEmail = ""
                        }
                    };
    
                    uploader.SendAnonymousReport(SendRequestState.GetClientLib(), state.GetApplication(), state.GetExceptionDescription(false));
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Failed to send crash report :(");
            }

        }

        private static int RunWithContainer(CommandLineParser parser, Bootstrapper container, IPersistance persistance)
        {
            var returnCode = 0;
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
                        ? new[] {ServiceEnvironmentManagement.MachineQualifiedServiceAccountName(parser.Target)}
                        : new string[0]).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                harness.RunProcess(environment =>
                {
                    if (parser.Service)
                    {
                        RunService(parser, environment, Logger);
                        returnCode = 0;
                    }
                    else
                    {
                        returnCode = RunProcess(parser, environment);
                    }
                }, servicePrincipal);

                CalculateAndDisplayResults(persistance.CoverageSession, parser);
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
            return returnCode;
        }

        /// <summary>
        /// Terminates current W3SVC hosting process (svchost.exe -k iissvcs)
        /// </summary>
        /// <returns>Returns wether the svchost.exe was restarted by the services.exe process or not</returns>
        private static bool TerminateCurrentW3SvcHost()
        {
            var processName = "svchost.exe";
            string wmiQuery = string.Format("select CommandLine, ProcessId from Win32_Process where Name='{0}'", processName);
            var searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            foreach (var retObject in retObjectCollection)
            {
                var cmdLine = (string)retObject["CommandLine"];
                if (cmdLine.EndsWith("-k iissvcs"))
                {
                    var proc = (int)retObject["ProcessId"];

                    // Terminate, the restart is done automatically
                    Logger.InfoFormat("Stopping svchost with pid '{0}'", proc);
                    try
                    {
                        Process.GetProcessById(proc).Kill();
                        Logger.InfoFormat("svchost with pid '{0}' was stopped succcesfully", proc);
                    }
                    catch (Exception e)
                    {
                        Logger.InfoFormat("Unable to stop svchost with pid '{0}' IIS profiling may not work: {1}", proc, e.Message);
                    }
                }
            }

            // Wait three seconds for the svchost to start
            // TODO, make this configurable
            var secondstowait = 3;

            // Wait for successfull restart of the svchost
            Stopwatch s = new Stopwatch();
            s.Start();
            bool found = false;
            while (s.Elapsed < TimeSpan.FromSeconds(secondstowait))
            {
                retObjectCollection = searcher.Get();
                foreach (var retObject in retObjectCollection)
                {
                    var cmdLine = (string)retObject["CommandLine"] ?? string.Empty;
                    if (cmdLine.EndsWith("-k iissvcs"))
                    {
                        var proc = (uint)retObject["ProcessId"];
                        Logger.InfoFormat("New svchost for w3svc with pid '{0}' was started", proc);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            s.Stop();

            // Return the found state
            return found;
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

                    if (parser.Target.ToLower().Equals("w3svc"))
                    {
                        // Service will not automatically start
                        if (! TerminateCurrentW3SvcHost() ||
                            !ServiceEnvironmentManagementEx.IsServiceStartAutomatic(parser.Target))
                        {
                            service.Start();
                        }
                    }
                    else
                    {
                        service.Start();
                    }
                    logger.InfoFormat("Service starting '{0}'", parser.Target);
                    service.WaitForStatus(ServiceControllerStatus.Running, parser.ServiceStartTimeout);
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

                // Stopping w3svc host
                if (parser.Target.ToLower().Equals("w3svc"))
                {
                    logger.InfoFormat("Stopping svchost to clean up environment variables for {0}", parser.Target);
                    if (ServiceEnvironmentManagementEx.IsServiceStartAutomatic(parser.Target))
                    {
                        logger.InfoFormat("Please note that the 'w3svc' service may automatically start");
                    }
                    TerminateCurrentW3SvcHost();
                }
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

            if (parser.DiagMode)
                startInfo.EnvironmentVariables[@"OpenCover_Profiler_Diagnostics"] = "true";

            startInfo.Arguments = parser.TargetArgs;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = parser.TargetDir;

            try
            {
                var process = Process.Start(startInfo);
                process.WaitForExit();

                if (parser.ReturnTargetCode)
                    returnCode = process.ExitCode;
                return returnCode;
            }
            catch (Exception)
            {
                Logger.ErrorFormat("Failed to execute the following command '{0} {1}'", startInfo.FileName, startInfo.Arguments);
            }
            return 1;
        }

        private class Results
        {
            public int altTotalClasses;
            public int altVisitedClasses;
            public int altTotalMethods;
            public int altVisitedMethods;
            public List<string> unvisitedClasses = new List<string>();
            public List<string> unvisitedMethods = new List<string>();
        }

        private static void CalculateAndDisplayResults(CoverageSession coverageSession, ICommandLine parser)
        {
            if (!Logger.IsInfoEnabled)
                return;

            var results = new Results();

            if (coverageSession.Modules != null)
            {
                CalculateResults(coverageSession, results);
            }

            DisplayResults(coverageSession, parser, results);
        }

        private static void CalculateResults(CoverageSession coverageSession, Results results)
        {
            foreach (var @class in
                                from module in coverageSession.Modules.Where(x => x.Classes != null)
                                from @class in module.Classes.Where(c => !c.ShouldSerializeSkippedDueTo())
                                select @class)
            {
                if (@class.Methods == null)
                    continue;

                if (!@class.Methods.Any(x => !x.ShouldSerializeSkippedDueTo() && x.SequencePoints.Any(y => y.VisitCount > 0))
                    && @class.Methods.Any(x => x.FileRef != null))
                {
                    results.unvisitedClasses.Add(@class.FullName);
                }

                if (@class.Methods.Any(x => x.Visited))
                {
                    results.altVisitedClasses += 1;
                    results.altTotalClasses += 1;
                }
                else if (@class.Methods.Any())
                {
                    results.altTotalClasses += 1;
                }

                foreach (var method in @class.Methods.Where(x => !x.ShouldSerializeSkippedDueTo()))
                {
                    if (method.FileRef != null && !method.SequencePoints.Any(x => x.VisitCount > 0))
                        results.unvisitedMethods.Add(string.Format("{0}", method.FullName));

                    results.altTotalMethods += 1;
                    if (method.Visited)
                    {
                        results.altVisitedMethods += 1;
                    }
                }
            }
        }

        private static void DisplayResults(CoverageSession coverageSession, ICommandLine parser, Results results)
        {
            if (coverageSession.Summary.NumClasses > 0)
            {
                Logger.InfoFormat("Visited Classes {0} of {1} ({2})", coverageSession.Summary.VisitedClasses,
                                  coverageSession.Summary.NumClasses, Math.Round(coverageSession.Summary.VisitedClasses * 100.0 / coverageSession.Summary.NumClasses, 2));
                Logger.InfoFormat("Visited Methods {0} of {1} ({2})", coverageSession.Summary.VisitedMethods,
                                  coverageSession.Summary.NumMethods, Math.Round(coverageSession.Summary.VisitedMethods * 100.0 / coverageSession.Summary.NumMethods, 2));
                Logger.InfoFormat("Visited Points {0} of {1} ({2})", coverageSession.Summary.VisitedSequencePoints,
                                  coverageSession.Summary.NumSequencePoints, coverageSession.Summary.SequenceCoverage);
                Logger.InfoFormat("Visited Branches {0} of {1} ({2})", coverageSession.Summary.VisitedBranchPoints,
                                  coverageSession.Summary.NumBranchPoints, coverageSession.Summary.BranchCoverage);

                Logger.InfoFormat("");
                Logger.InfoFormat(
                    "==== Alternative Results (includes all methods including those without corresponding source) ====");
                Logger.InfoFormat("Alternative Visited Classes {0} of {1} ({2})", results.altVisitedClasses,
                                  results.altTotalClasses, results.altTotalClasses == 0 ? 0 : Math.Round(results.altVisitedClasses * 100.0 / results.altTotalClasses, 2));
                Logger.InfoFormat("Alternative Visited Methods {0} of {1} ({2})", results.altVisitedMethods,
                                  results.altTotalMethods, results.altTotalMethods == 0 ? 0 : Math.Round(results.altVisitedMethods * 100.0 / results.altTotalMethods, 2));

                if (parser.ShowUnvisited)
                {
                    Logger.InfoFormat("");
                    Logger.InfoFormat("====Unvisited Classes====");
                    foreach (var unvisitedClass in results.unvisitedClasses)
                    {
                        Logger.InfoFormat(unvisitedClass);
                    }

                    Logger.InfoFormat("");
                    Logger.InfoFormat("====Unvisited Methods====");
                    foreach (var unvisitedMethod in results.unvisitedMethods)
                    {
                        Logger.InfoFormat(unvisitedMethod);
                    }
                }
            }
            else
            {
                Logger.InfoFormat("No results, this could be for a number of reasons. The most common reasons are:");
                Logger.InfoFormat("    1) missing PDBs for the assemblies that match the filter please review the");
                Logger.InfoFormat("    output file and refer to the Usage guide (Usage.rtf) about filters.");
                Logger.InfoFormat("    2) the profiler may not be registered correctly, please refer to the Usage");
                Logger.InfoFormat("    guide and the -register switch.");
            }
        }

        private static bool GetFullOutputFile(CommandLineParser parser, out string outputFile)
        {
            try
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables(parser.OutputFile));
            }
            catch (Exception ex)
            {
                outputFile = null;
                System.Console.WriteLine("Invalid `outputFile` supplied: {0}", ex.Message);
                return false;
            }

            string directoryName;
            try
            {
                directoryName = Path.GetDirectoryName(outputFile);
            }
            catch (PathTooLongException pathTooLongEx)
            {
                System.Console.WriteLine("Output file path exceeds system limits, please use another. {0} File path is: {1}",
                        pathTooLongEx.Message, outputFile);
                return false;
            }

            if (!Directory.Exists(directoryName ?? string.Empty))
            {
                System.Console.WriteLine("Output folder does not exist; please create it and make sure appropriate permissions are set.");
                return false;
            }
            return true;
        }


        private static IFilter BuildFilter(CommandLineParser parser)
        {
            var filter = Filter.BuildFilter(parser);
            if (!string.IsNullOrWhiteSpace(parser.FilterFile))
            {
                if (!File.Exists(parser.FilterFile.Trim()))
                    System.Console.WriteLine("FilterFile '{0}' cannot be found - have you specified your arguments correctly?", parser.FilterFile);
                else
                {
                    var filters = File.ReadAllLines(parser.FilterFile);
                    filters.ToList().ForEach(filter.AddFilter);
                }
            }
            else
            {
                if (parser.Filters.Count == 0)
                    filter.AddFilter("+[*]*");
            }

            return filter;
        }

        private static IPerfCounters CreatePerformanceCounter(CommandLineParser parser)
        {
            if (parser.EnablePerformanceCounters)
            {
                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    return new PerfCounters();
                }
                Logger.Error("You must be running as an Administrator to enable performance counters.");
                throw new ExitApplicationWithoutReportingException();
            }
            return new NullPerfCounter();
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


                if (parser.PrintVersion)
                {
                    var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        Logger.Warn("No entry assembly, running from unmanaged application");
                    }
                    else
                    {
                        var version = entryAssembly.GetName().Version;
                        System.Console.WriteLine("OpenCover version {0}", version);
                        if (args.Length == 1)
                            return false;
                    }
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
                            System.Console.WriteLine("Service '{0}' found", name);
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
                System.Console.WriteLine("");
                System.Console.WriteLine("Incorrect Arguments: {0}", ex.Message);
                System.Console.WriteLine("");
                System.Console.WriteLine(parser.Usage());
                return false;
            }
            return true;
        }
    }
}
