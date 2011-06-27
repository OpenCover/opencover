//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
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
        private string _fileName;

        public void Initialise(string fileName)
        {
            _fileName = fileName;
        }

        public virtual void Commit()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                var fs = new FileStream(_fileName, FileMode.Create);
                var writer = new StreamWriter(fs, new UTF8Encoding());
                serializer.Serialize(writer, CoverageSession);
                writer.Close();

                var totalClasses = 0;
                var visitedClasses = 0;

                var totalSeqPoint = 0;
                var visitedSeqPoint = 0;
                var totalMethods = 0;
                var visitedMethods = 0;

                if (CoverageSession.Modules != null)
                {
                    foreach (var @class in
                        from module in CoverageSession.Modules
                        from @class in module.Classes
                        select @class)
                    {
                        if ((@class.Methods.Where(x => x.FileRef != null).Any()))
                        {
                            totalClasses += 1;
                        }
                        visitedClasses += (@class.Methods.Where(x => x.SequencePoints.Where(y => y.VisitCount > 0).Any()).Any()) ? 1 : 0;
                        if (@class.Methods == null) continue;

                        foreach (var method in @class.Methods)
                        {
                            totalMethods += (method.FileRef != null) ? 1 : 0;
                            visitedMethods += (method.SequencePoints.Where(x => x.VisitCount > 0).Any()) ? 1 : 0;
                            totalSeqPoint += method.SequencePoints.Count();
                            visitedSeqPoint += method.SequencePoints.Where(pt => pt.VisitCount != 0).Count();
                        }
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

    }
}
