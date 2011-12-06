//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var returnCodeOffset = 0;
            try
            {
                CommandLineParser parser;
                if (!ParseCommandLine(args, out parser)) return parser.ReturnCodeOffset + 1;
                returnCodeOffset = parser.ReturnCodeOffset;
                var filter = BuildFilter(parser);

                string outputFile;
                if (!GetFullOutputFile(parser, out outputFile)) return returnCodeOffset + 1;

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

                    DisplayResults(persistance, parser);

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
                returnCode = returnCodeOffset + 1;
            }

            return returnCode;
        }

        private static void DisplayResults(IPersistance persistance, ICommandLine parser)
        {
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
                System.Console.WriteLine("Visited Classes {0} of {1} ({2})", visitedClasses,
                                  totalClasses, (double)visitedClasses * 100.0 / (double)totalClasses);
                System.Console.WriteLine("Visited Methods {0} of {1} ({2})", visitedMethods,
                                  totalMethods, (double)visitedMethods * 100.0 / (double)totalMethods);
                System.Console.WriteLine("Visited Points {0} of {1} ({2})", visitedSeqPoint,
                                  totalSeqPoint, (double)visitedSeqPoint * 100.0 / (double)totalSeqPoint);
                System.Console.WriteLine("Visited Branches {0} of {1} ({2})", visitedBrPoint,
                                  totalBrPoint, (double)visitedBrPoint * 100.0 / (double)totalBrPoint);

                System.Console.WriteLine("");
                System.Console.WriteLine(
                    "==== Alternative Results (includes all methods including those without corresponding source) ====");
                System.Console.WriteLine("Alternative Visited Classes {0} of {1} ({2})", altVisitedClasses,
                                  altTotalClasses, (double)altVisitedClasses * 100.0 / (double)altTotalClasses);
                System.Console.WriteLine("Alternative Visited Methods {0} of {1} ({2})", altVisitedMethods,
                                  altTotalMethods, (double)altVisitedMethods * 100.0 / (double)altTotalMethods);

                if (parser.ShowUnvisited)
                {
                    System.Console.WriteLine("");
                    System.Console.WriteLine("====Unvisited Classes====");
                    foreach (var unvisitedClass in unvisitedClasses)
                    {
                        System.Console.WriteLine(unvisitedClass);
                    }

                    System.Console.WriteLine("");
                    System.Console.WriteLine("====Unvisited Methods====");
                    foreach (var unvisitedMethod in unvisitedMethods)
                    {
                        System.Console.WriteLine(unvisitedMethod);
                    }
                }
            }
            else
            {
                System.Console.WriteLine("No results - no assemblies that matched the supplied filter were instrumented (missing PDBs?)");
            }
        }

        private static bool GetFullOutputFile(CommandLineParser parser, out string outputFile)
        {
            outputFile = Path.Combine(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables(parser.OutputFile));
            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                System.Console.WriteLine(
                    "Output folder does not exist; please create it and make sure appropriate permissions are set.");
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

                if (!File.Exists(Environment.ExpandEnvironmentVariables(parser.Target)))
                {
                    System.Console.WriteLine("Target {0} cannot be found - have you specified your arguments correctly?", parser.Target);
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
