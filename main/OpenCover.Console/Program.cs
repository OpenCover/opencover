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
using System.ServiceProcess;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
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

                var container = new Bootstrapper(logger);
                var persistance = new FilePersistance(parser, logger);

                container.Initialise(filter, parser, persistance);
                persistance.Initialise(outputFile);
                var registered = false;

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
                                               returnCode = 0;
                                               if (parser.Service)
                                               {
                                                   RunService(parser, environment, logger);
                                               }
                                               else
                                               {
                                                   returnCode = RunProcess(parser, environment);
                                               }
                                           });

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
                        ProfilerRegistration.Unregister(parser.UserRegistration);
                }
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
            var service = new ServiceController(parser.Target);
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                logger.ErrorFormat("The service '{0}' is already running. The profiler cannot attach to an already running service.", 
                    parser.Target);
                return;
            }

            // now to set the environment variables
            var profilerEnvironment = new StringDictionary();
            environment(profilerEnvironment);

            // now start the service
            service.Start();
            logger.InfoFormat("Service starting '{0}'", parser.Target);
            service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
            logger.InfoFormat("Service started '{0}'", parser.Target);

            // and wait for it to stop
            service.WaitForStatus(ServiceControllerStatus.Stopped);
            logger.InfoFormat("Service stopped '{0}'", parser.Target);
        }

        private static int RunProcess(CommandLineParser parser, Action<StringDictionary> environment)
        {
            var returnCode = 0;
            var startInfo =
                new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, parser.Target));
            environment(startInfo.EnvironmentVariables);

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

            var totalSeqPoint = 0;
            var visitedSeqPoint = 0;
            var totalMethods = 0;
            var visitedMethods = 0;

            var altTotalMethods = 0;
            var altVisitedMethods = 0;

            var totalBrPoint = 0;
            var visitedBrPoint = 0;

            var unvisitedClasses = new List<string>();
            var unvisitedMethods = new List<string>();

            if (CoverageSession.Modules != null)
            {
                foreach (var @class in
                    from module in CoverageSession.Modules
                    from @class in module.Classes
                    select @class)
                {
                    if ((@class.Methods.Where(x => x.SequencePoints.Where(y => y.VisitCount > 0).Any()).Any()))
                    {
                        visitedClasses += 1;
                        totalClasses += 1;
                    }
                    else if ((@class.Methods.Where(x => x.FileRef != null).Any()))
                    {
                        totalClasses += 1;
                        unvisitedClasses.Add(@class.FullName);
                    }

                    if (@class.Methods.Where(x => x.Visited).Any())
                    {
                        altVisitedClasses += 1;
                        altTotalClasses += 1;
                    }
                    else if (@class.Methods.Any())
                    {
                        altTotalClasses += 1;
                    }

                    if (@class.Methods == null) continue;

                    foreach (var method in @class.Methods)
                    {
                        if ((method.SequencePoints.Where(x => x.VisitCount > 0).Any()))
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

                        totalSeqPoint += method.SequencePoints.Count();
                        visitedSeqPoint += method.SequencePoints.Where(pt => pt.VisitCount != 0).Count();

                        totalBrPoint += method.BranchPoints.Count();
                        visitedBrPoint += method.BranchPoints.Where(pt => pt.VisitCount != 0).Count();
                    }
                }
            }

            if (totalClasses > 0)
            {
                
                logger.InfoFormat("Visited Classes {0} of {1} ({2})", visitedClasses,
                                  totalClasses, (double)visitedClasses * 100.0 / (double)totalClasses);
                logger.InfoFormat("Visited Methods {0} of {1} ({2})", visitedMethods,
                                  totalMethods, (double)visitedMethods * 100.0 / (double)totalMethods);
                logger.InfoFormat("Visited Points {0} of {1} ({2})", visitedSeqPoint,
                                  totalSeqPoint, (double)visitedSeqPoint * 100.0 / (double)totalSeqPoint);
                logger.InfoFormat("Visited Branches {0} of {1} ({2})", visitedBrPoint,
                                  totalBrPoint, (double)visitedBrPoint * 100.0 / (double)totalBrPoint);

                logger.InfoFormat("");
                logger.InfoFormat(
                    "==== Alternative Results (includes all methods including those without corresponding source) ====");
                logger.InfoFormat("Alternative Visited Classes {0} of {1} ({2})", altVisitedClasses,
                                  altTotalClasses, (double)altVisitedClasses * 100.0 / (double)altTotalClasses);
                logger.InfoFormat("Alternative Visited Methods {0} of {1} ({2})", altVisitedMethods,
                                  altTotalMethods, (double)altVisitedMethods * 100.0 / (double)altTotalMethods);

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
                logger.InfoFormat("No results - no assemblies that matched the supplied filter were instrumented (missing PDBs?)");
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

            if (parser.Filters.Count == 0)
            {
                filter.AddFilter("+[*]*");
            }
            else
            {
                parser.Filters.ForEach(filter.AddFilter);
            }

            filter.AddAttributeExclusionFilters(parser.AttributeExclusionFilters.ToArray());
            filter.AddFileExclusionFilters(parser.FileExclusionFilters.ToArray());

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
                    return true;
                }

                if (parser.Service)
                {
                    try
                    {
                        var service = new ServiceController(parser.Target);
                        var name = service.DisplayName;
                    }
                    catch (Exception)
                    {
                        System.Console.WriteLine("Service '{0}' cannot be found - have you specified your arguments correctly?", parser.Target);
                        return false;
                    }                    
                }
                else if (!File.Exists(Environment.ExpandEnvironmentVariables(parser.Target)))
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
