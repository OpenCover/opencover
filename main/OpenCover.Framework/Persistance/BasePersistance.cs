using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public abstract class BasePersistance : IPersistance
    {
        protected readonly ICommandLine _commandLine;
        private uint _trackedMethodId;

        public BasePersistance(ICommandLine commandLine)
        {
            _commandLine = commandLine;
            CoverageSession = new CoverageSession();
            _trackedMethodId = 0;
        }

        public CoverageSession CoverageSession { get; private set; }

        public void PersistModule(Module module)
        {
            if (module == null) return;
            module.Classes = module.Classes ?? new Class[0];
            if (_commandLine.MergeByHash)
            {
                var modules = CoverageSession.Modules ?? new Module[0];
                var existingModule = modules.FirstOrDefault(x => x.ModuleHash == module.ModuleHash);
                if (existingModule!=null)
                {
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
            return CoverageSession.Modules.Any(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)) && 
                    !x.ShouldSerializeSkippedDueTo());
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
                var branchPoints = method.BranchPoints ?? new BranchPoint[0];

                if (method.MethodPoint != null)
                {
                    method.Visited = (method.MethodPoint.VisitCount > 0);
                }

                var numTotBrPoint = branchPoints.Count();
                var numVisBrPoint = branchPoints.Count(pt => pt.VisitCount != 0);
                var numTotSeqPoint = sequencePoints.Count();
                var numVisSeqPoint = sequencePoints.Count(pt => pt.VisitCount != 0);

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
            if (method !=null && method.SequencePoints != null)
            {
                System.Diagnostics.Debug.WriteLine("Getting Sequence points for {0}({1})", method.Name, method.MetadataToken);
                var points = new List<InstrumentationPoint>();
                if (!(method.MethodPoint is SequencePoint))
                    points.Add(method.MethodPoint);
                points.AddRange(method.SequencePoints);
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
            if (method != null && method.BranchPoints != null)
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
                var spid = BitConverter.ToUInt32(data, idx);
                if (spid < (uint)MSG_IdType.IT_MethodEnter) 
                    InstrumentationPoint.AddCount(spid, _trackedMethodId);
                else
                {
                    var tmId = spid & (uint)MSG_IdType.IT_Mask;
                    _trackedMethodId = (spid & (uint)MSG_IdType.IT_MethodEnter) != 0 ? tmId : 0;
                }
            }
        }

        public bool GetTrackingMethod(string modulePath, string assemblyName, int functionToken, out uint uniqueId)
        {
            uniqueId = 0;
            foreach (var module in CoverageSession.Modules
                .Where(x => x.TrackedMethods != null)
                .Where(x => x.Aliases.Contains(modulePath)))
            {
                foreach (var trackedMethod in module.TrackedMethods)
                {
                    if (trackedMethod.MetadataToken == functionToken)
                    {
                        uniqueId = trackedMethod.UniqueId;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}