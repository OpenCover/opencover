using System;
using System.Collections.Generic;
using System.Linq;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    
    public class BasePersistance : IPersistance
    {
        private readonly ICommandLine _commandLine;

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
        }

        public bool GetSequencePointsForFunction(string modulePath, int functionToken, out InstrumentationPoint[] sequencePoints)
        {
            sequencePoints = new InstrumentationPoint[0];
            Class @class;
            var method = GetMethod(modulePath, functionToken, out @class);
            if (method !=null)
            {
                var points = new List<InstrumentationPoint>();
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
            if (method != null)
            {
                branchPoints = method.BranchPoints.ToArray();
                return true;
            }
            return false;
        }

        private Method GetMethod(string modulePath, int functionToken, out Class @class)
        {
            @class = null;
            //c = null;
            if (CoverageSession.Modules == null) return null;
            var module = CoverageSession.Modules.FirstOrDefault(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)));
            if (module == null) return null;
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
                SequencePoint.AddCount(BitConverter.ToUInt32(data, idx));   
            }
        }
    }
}