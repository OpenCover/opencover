using System.Collections.Generic;
using System.Linq;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    /// <summary>
    /// This class is temporary until I decide 
    /// on a proper persistance framework.
    /// </summary>
    public class BasePersistance : IPersistance
    {
        public BasePersistance()
        {
            CoverageSession = new CoverageSession();
        }

        public CoverageSession CoverageSession { get; private set; }

        public void PersistModule(Module module)
        {
            var list = new List<Module>(CoverageSession.Modules ?? new Module[0]) { module };
            CoverageSession.Modules = list.ToArray();
        }

        public bool IsTracking(string moduleName)
        {
            if (CoverageSession.Modules == null) return false;
            return CoverageSession.Modules.Any(x => x.FullName == moduleName);
        }

        public virtual void Commit()
        {
        }

        public bool GetSequencePointsForFunction(string moduleName, int functionToken, out SequencePoint[] sequencePoints)
        {
            sequencePoints = new SequencePoint[0];
            if (CoverageSession.Modules == null) return false;
            var module = CoverageSession.Modules.Where(x => x.FullName == moduleName).FirstOrDefault();
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
                foreach (var sequencePoint in from module in CoverageSession.Modules
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