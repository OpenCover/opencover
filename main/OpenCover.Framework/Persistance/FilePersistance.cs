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
    /// <summary>
    /// This class is temporary until I decide 
    /// on a proper persistance framework.
    /// </summary>
    public class FilePersistance : IPersistance
    {
        private string _fileName;
        private CoverageSession _session;

        public void Initialise(string fileName)
        {
            _fileName = fileName;
            _session = new CoverageSession();
        }

        public void PersistModule(Module module)
        {
            var list = new List<Module>(_session.Modules ?? new Module[0]) {module};
            _session.Modules = list.ToArray();
        }

        public bool IsTracking(string moduleName)
        {
            if (_session.Modules == null) return false;
            return _session.Modules.Any(x => x.FullName == moduleName);
        }

        public void Commit()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                var fs = new FileStream(_fileName, FileMode.Create);
                var writer = new StreamWriter(fs, new UTF8Encoding());
                serializer.Serialize(writer, _session);
                writer.Close();

                var totalClasses = 0;
                var visitedClasses = 0;

                var totalSeqPoint = 0;
                var visitedSeqPoint = 0;
                var totalMethods = 0;
                var visitedMethods = 0;

                foreach (var @class in
                    from module in _session.Modules
                    from @class in module.Classes
                    select @class)
                {
                    //Console.WriteLine("{0}",@class.FullName);
                    if ((@class.Methods.Where(x=>x.SequencePoints.Count()>0).Any()))
                    {
                        totalClasses += 1;
                        //Console.WriteLine("{0} : {1}", @class.FullName, (@class.Methods.Where(x => x.SequencePoints.Where(y => y.VisitCount > 0).Any()).Any()));
                    }
                    visitedClasses += (@class.Methods.Where(x => x.SequencePoints.Where(y => y.VisitCount > 0).Any()).Any()) ? 1 : 0;

                    foreach (var method in @class.Methods)
                    {
                        totalMethods += (method.SequencePoints.Count() > 0) ? 1 : 0;
                        visitedMethods += (method.SequencePoints.Where(x => x.VisitCount > 0).Any()) ? 1 : 0;
                        totalSeqPoint += method.SequencePoints.Count();
                        visitedSeqPoint += method.SequencePoints.Where(pt => pt.VisitCount != 0).Count();
                    }
                }

                Console.WriteLine("Visited Classes {0} of {1} ({2})", visitedClasses, totalClasses, (double)visitedClasses * 100.0 / (double)totalClasses);
                Console.WriteLine("Visited Methods {0} of {1} ({2})", visitedMethods, totalMethods, (double)visitedMethods * 100.0 / (double)totalMethods);
                Console.WriteLine("Visited Points {0} of {1} ({2})", visitedSeqPoint, totalSeqPoint, (double)visitedSeqPoint * 100.0 / (double)totalSeqPoint);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public bool GetSequencePointsForFunction(string moduleName, int functionToken, out SequencePoint[] sequencePoints)
        {
            sequencePoints = new SequencePoint[0];
            if (_session.Modules == null) return false;
            var module = _session.Modules.Where(x => x.FullName == moduleName).FirstOrDefault();
            if (module == null) return false;
            foreach (var method in module.Classes.SelectMany(@class => @class.Methods.Where(method => method.MetadataToken == functionToken)))
            {
                if (method == null) continue;
                sequencePoints = method.SequencePoints;
                return true;
            }
            return false;       
        }

        public void SaveVisitPoints(VisitPoint[] visitPoints)
        {
            var summary = from point in visitPoints
                          group point by point.UniqueId into counts
                          let count = counts.Count()
                          select new { point = counts.Key, Count = count };

            foreach (var sum in summary)
            {
                var sum1 = sum;
                foreach (var sequencePoint in from module in _session.Modules
                                              from @class in module.Classes
                                              from method in @class.Methods
                                              from sequencePoint in method.SequencePoints
                                              where sequencePoint.UniqueSequencePoint == sum1.point
                                              select sequencePoint)
                {
                    sequencePoint.VisitCount += sum.Count;
                }
            }
        }
    }
}
