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
using System.Text;
using System.Xml.Serialization;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public class FilePersistance : BasePersistance
    {

        public FilePersistance(ICommandLine commandLine) : base(commandLine)
        {
        }

        private string _fileName;

        public void Initialise(string fileName)
        {
            _fileName = fileName;
        }

        public override void Commit()
        {
            try
            {
                Console.WriteLine("Committing....");
                foreach (var sequencePoint in from module in CoverageSession.Modules
                                              from @class in module.Classes
                                              from method in @class.Methods
                                              from sequencePoint in method.SequencePoints
                                              select sequencePoint)
                {
                    sequencePoint.VisitCount = InstrumentationPoint.GetCount(sequencePoint.UniqueSequencePoint);
                }

                foreach (var branchPoint in from module in CoverageSession.Modules
                                            from @class in module.Classes
                                            from method in @class.Methods
                                            from branchPoint in method.BranchPoints
                                            select branchPoint)
                {
                    branchPoint.VisitCount = InstrumentationPoint.GetCount(branchPoint.UniqueSequencePoint);
                }

                foreach (var method in from module in CoverageSession.Modules
                                       from @class in module.Classes
                                       from method in @class.Methods
                                       select method)
                {
                    method.MethodPoint.VisitCount = InstrumentationPoint.GetCount(method.MethodPoint.UniqueSequencePoint);
                    method.Visited = (method.MethodPoint.VisitCount > 0);

                    var numTotBrPoint = method.BranchPoints.Count();
                    var numVisBrPoint = method.BranchPoints.Where(pt => pt.VisitCount != 0).Count();
                    var numTotSeqPoint = method.SequencePoints.Count();
                    var numVisSeqPoint = method.SequencePoints.Where(pt => pt.VisitCount != 0).Count();

                    if (numTotSeqPoint > 0)
                        method.SequenceCoverage = (numVisSeqPoint * 100) / numTotSeqPoint;

                    if (numTotBrPoint == 0)
                    {
                        if (numVisSeqPoint > 0) method.BranchCoverage = 100;
                    }
                    else
                        method.BranchCoverage = (numVisBrPoint * 100) / numTotBrPoint;
                }

                var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                var fs = new FileStream(_fileName, FileMode.Create);
                var writer = new StreamWriter(fs, new UTF8Encoding());
                serializer.Serialize(writer, CoverageSession);
                writer.Close();

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

                        if (@class.Methods.Where(x=>x.Visited).Any())
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

                Console.WriteLine("Visited Classes {0} of {1} ({2})", visitedClasses, 
                    totalClasses, (double)visitedClasses * 100.0 / (double)totalClasses);
                Console.WriteLine("Visited Methods {0} of {1} ({2})", visitedMethods, 
                    totalMethods, (double)visitedMethods * 100.0 / (double)totalMethods);
                Console.WriteLine("Visited Points {0} of {1} ({2})", visitedSeqPoint, 
                    totalSeqPoint, (double)visitedSeqPoint * 100.0 / (double)totalSeqPoint);
                Console.WriteLine("Visited Branches {0} of {1} ({2})", visitedBrPoint, 
                    totalBrPoint, (double)visitedBrPoint * 100.0 / (double)totalBrPoint);

                Console.WriteLine("");
                Console.WriteLine("==== Alternative Results (includes all methods including those without corresponding source) ====");
                Console.WriteLine("Alternative Visited Classes {0} of {1} ({2})", altVisitedClasses, 
                    altTotalClasses, (double)altVisitedClasses * 100.0 / (double)altTotalClasses);
                Console.WriteLine("Alternative Visited Methods {0} of {1} ({2})", altVisitedMethods, 
                    altTotalMethods, (double)altVisitedMethods * 100.0 / (double)altTotalMethods);

                Console.WriteLine("");
                Console.WriteLine("====Unvisited Classes====");
                foreach (var unvisitedClass in unvisitedClasses)
                {
                    Console.WriteLine(unvisitedClass);
                }

                Console.WriteLine("");
                Console.WriteLine("====Unvisited Methods====");
                foreach (var unvisitedMethod in unvisitedMethods)
                {
                    Console.WriteLine(unvisitedMethod);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            }
        }

        
    }
}
