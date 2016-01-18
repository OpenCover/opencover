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


    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("OpenCover");

        /// <summary>
        /// This is the initial console harness - it may become the full thing
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            var returnCode = 0;
            var returnCodeOffset = 0;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                //throw new NullReferenceException();

                CommandLineParser parser;
                if (!ParseCommandLine(args, out parser)) return parser.ReturnCodeOffset + 1;


                LogManager.GetRepository().Threshold = parser.LogLevel;

                returnCodeOffset = parser.ReturnCodeOffset;
                var filter = BuildFilter(parser);
                var perfCounter = CreatePerformanceCounter(parser);

                string outputFile;
                if (!GetFullOutputFile(parser, out outputFile)) return returnCodeOffset + 1;

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
            catch (ExitApplicationWithoutReportingException eex)
            {
                Logger.ErrorFormat("If you are unable to resolve the issue please contact the OpenCover development team");
                Logger.ErrorFormat("see https://www.github.com/opencover/opencover/issues");
                returnCode = returnCodeOffset + 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal("At: Program.Main");
                Logger.FatalFormat("An {0} occured: {1}", ex.GetType(), ex.Message);
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
                Logger.FatalFormat("An {0} occured: {1}", ex.GetType(), ex.Message);
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
                            ApplicationGuid = new Guid("e3933a4b-368b-4256-ad42-777bc60a9558"),
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

                DisplayResults(persistance.CoverageSession, parser, Logger);
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
        /// <param name="logger"></param>
        /// <returns>Returns wether the svchost.exe was restarted by the services.exe process or not</returns>
        private static bool TerminateCurrentW3SvcHost(ILog logger)
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
                    logger.InfoFormat("Stopping svchost with pid '{0}'", proc);
                    try
                    {
                        Process.GetProcessById(proc).Kill();
                        logger.InfoFormat("svchost with pid '{0}' was stopped succcesfully", proc);
                    }
                    catch (Exception e)
                    {
                        logger.InfoFormat("Unable to stop svchost with pid '{0}' IIS profiling may not work: {1}", proc, e.Message);
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
                        logger.InfoFormat("New svchost for w3svc with pid '{0}' was started", proc);
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
                        if (! TerminateCurrentW3SvcHost(logger) ||
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
                    TerminateCurrentW3SvcHost(logger);
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
            catch (Exception ex)
            {
                Logger.ErrorFormat("Failed to execute the following command '{0} {1}'", startInfo.FileName, startInfo.Arguments);
            }
            return 1;
        }

        private static void DisplayResults(CoverageSession coverageSession, ICommandLine parser, ILog logger)
        {
            if (!logger.IsInfoEnabled) return;

            var altTotalClasses = 0;
            var altVisitedClasses = 0;

            var altTotalMethods = 0;
            var altVisitedMethods = 0;

            var unvisitedClasses = new List<string>();
            var unvisitedMethods = new List<string>();

            if (coverageSession.Modules != null)
            {
                foreach (var @class in
                    from module in coverageSession.Modules.Where(x=>x.Classes != null)
                    from @class in module.Classes.Where(c => !c.ShouldSerializeSkippedDueTo())
                    select @class)
                {
                    if (@class.Methods == null) continue;

                    if ((@class.Methods.Any(x => !x.ShouldSerializeSkippedDueTo() && x.SequencePoints.Any(y => y.VisitCount > 0))))
                    {
                    }
                    else if ((@class.Methods.Any(x => x.FileRef != null)))
                    {
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
                        }
                        else if (method.FileRef != null)
                        {
                            unvisitedMethods.Add(string.Format("{0}", method.FullName));
                        }

                        altTotalMethods += 1;
                        if (method.Visited)
                        {
                            altVisitedMethods += 1;
                        }
                    }
                }
            }

            if (coverageSession.Summary.NumClasses > 0)
            {
                logger.InfoFormat("Visited Classes {0} of {1} ({2})", coverageSession.Summary.VisitedClasses,
                                  coverageSession.Summary.NumClasses, Math.Round(coverageSession.Summary.VisitedClasses * 100.0 / coverageSession.Summary.NumClasses, 2));
                logger.InfoFormat("Visited Methods {0} of {1} ({2})", coverageSession.Summary.VisitedMethods,
                                  coverageSession.Summary.NumMethods, Math.Round(coverageSession.Summary.VisitedMethods * 100.0 / coverageSession.Summary.NumMethods, 2));
                logger.InfoFormat("Visited Points {0} of {1} ({2})", coverageSession.Summary.VisitedSequencePoints,
                                  coverageSession.Summary.NumSequencePoints, coverageSession.Summary.SequenceCoverage);
                logger.InfoFormat("Visited Branches {0} of {1} ({2})", coverageSession.Summary.VisitedBranchPoints,
                                  coverageSession.Summary.NumBranchPoints, coverageSession.Summary.BranchCoverage);

                logger.InfoFormat("");
                logger.InfoFormat(
                    "==== Alternative Results (includes all methods including those without corresponding source) ====");
                logger.InfoFormat("Alternative Visited Classes {0} of {1} ({2})", altVisitedClasses,
                                  altTotalClasses, altTotalClasses == 0 ? 0 : Math.Round(altVisitedClasses * 100.0 / altTotalClasses, 2));
                logger.InfoFormat("Alternative Visited Methods {0} of {1} ({2})", altVisitedMethods,
                                  altTotalMethods, altTotalMethods == 0 ? 0 : Math.Round(altVisitedMethods * 100.0 / altTotalMethods, 2));

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
