using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public abstract class BasePersistance : IPersistance
    {
        protected readonly ICommandLine _commandLine;

        public BasePersistance(ICommandLine commandLine)
        {
            _commandLine = commandLine;
            CoverageSession = new CoverageSession();
        }

        public CoverageSession CoverageSession { get; private set; }

        public void PersistModule(Module module)
        {
            if (_commandLine.MergeByHash)
            {
                var modules = CoverageSession.Modules ?? new Module[0];
                if (modules.Any(x => x.ModuleHash == module.ModuleHash))
                {
                    var existingModule = modules.First(x => x.ModuleHash == module.ModuleHash);
                    if (!existingModule.Aliases.Any(x=>x.Equals(module.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        existingModule.Aliases.Add(module.FullName); 
                    }
                    return;
                }
            }
            var list = new List<Module>(CoverageSession.Modules ?? new Module[0]) { module };
            CoverageSession.Modules = list.ToArray();
        }

        public bool IsTracking(string modulePath)
        {
            if (CoverageSession.Modules == null) return false;
            return CoverageSession.Modules.Any(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)));
        }

        public virtual void Commit()
        {
            PopulateInstrumentedPoints();
        }

        protected void PopulateInstrumentedPoints()
        {
            if (CoverageSession.Modules == null) return;
           
            foreach (var method in from module in CoverageSession.Modules
                                   from @class in module.Classes ?? new Class[0]
                                   from method in @class.Methods ?? new Method[0]
                                   select method)
            {
                var sequencePoints = method.SequencePoints ?? new SequencePoint[0];
                foreach (var sequencePoint in from sequencePoint in sequencePoints
                                          select sequencePoint)
                {
                    sequencePoint.VisitCount = InstrumentationPoint.GetCount(sequencePoint.UniqueSequencePoint);
                }

                var branchPoints = method.BranchPoints ?? new BranchPoint[0];
                foreach (var branchPoint in from branchPoint in branchPoints
                                            select branchPoint)
                {
                    branchPoint.VisitCount = InstrumentationPoint.GetCount(branchPoint.UniqueSequencePoint);
                }

                if (method.MethodPoint != null)
                {
                    method.MethodPoint.VisitCount =
                        InstrumentationPoint.GetCount(method.MethodPoint.UniqueSequencePoint);
                    method.Visited = (method.MethodPoint.VisitCount > 0);
                }

                var numTotBrPoint = branchPoints.Count();
                var numVisBrPoint = branchPoints.Where(pt => pt.VisitCount != 0).Count();
                var numTotSeqPoint = sequencePoints.Count();
                var numVisSeqPoint = sequencePoints.Where(pt => pt.VisitCount != 0).Count();

                if (numTotSeqPoint > 0)
                    method.SequenceCoverage = (numVisSeqPoint * 100) / numTotSeqPoint;

                if (numTotBrPoint == 0)
                {
                    if (numVisSeqPoint > 0) method.BranchCoverage = 100;
                }
                else
                    method.BranchCoverage = (numVisBrPoint * 100) / numTotBrPoint;
            }
        }

        public bool GetSequencePointsForFunction(string modulePath, int functionToken, out InstrumentationPoint[] sequencePoints)
        {
            sequencePoints = new InstrumentationPoint[0];
            Class @class;
            var method = GetMethod(modulePath, functionToken, out @class);
            if (method !=null)
            {
                System.Diagnostics.Debug.WriteLine("Getting Sequence points for {0}({1})", method.Name, method.MetadataToken);
                var points = new List<InstrumentationPoint>();
                points.AddRange(method.SequencePoints);
                if (points.Count == 0) points.Add(method.MethodPoint);
                sequencePoints = points.ToArray();
                return true;
            }
            return false;      
        }

        public bool GetBranchPointsForFunction(string modulePath, int functionToken, out BranchPoint[] branchPoints)
        {
            branchPoints = new BranchPoint[0];
            Class @class;
            var method = GetMethod(modulePath, functionToken, out @class);
            if (method != null)
            {
                System.Diagnostics.Debug.WriteLine("Getting Branch points for {0}({1})", method.Name, method.MetadataToken);
                branchPoints = method.BranchPoints.ToArray();
                return true;
            }
            return false;
        }

        private Method GetMethod(string modulePath, int functionToken, out Class @class)
        {
            @class = null;
            //c = null;
            if (CoverageSession.Modules == null) 
                return null;
            var module = CoverageSession.Modules.FirstOrDefault(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)));
            if (module == null) 
                return null;
            foreach (var c in module.Classes)
            {
                @class = c;
                foreach (var method in c.Methods)
                {
                    if (method.MetadataToken == functionToken) return method;
                }
            }
            @class = null;
            return null;
        }

        public string GetClassFullName(string modulePath, int functionToken)
        {
            Class @class;
            GetMethod(modulePath, functionToken, out @class);
            return @class != null ? @class.FullName : null;
        }

        public void SaveVisitData(byte[] data)
        {
            var nCount = BitConverter.ToUInt32(data, 0);
            for (int i = 0, idx = 4; i < nCount; i++, idx += 4)
            {
                InstrumentationPoint.AddCount(BitConverter.ToUInt32(data, idx));   
            }
        }
    }
}