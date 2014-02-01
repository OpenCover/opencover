using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;
using log4net;
using log4net.Core;

namespace OpenCover.Framework.Persistance
{
    /// <summary>
    /// A basic layer that aggregates the data
    /// </summary>
    public abstract class BasePersistance : IPersistance
    {
        protected readonly ICommandLine CommandLine;
        private readonly ILog _logger;
        private uint _trackedMethodId;
        private readonly Dictionary<Module, Dictionary<int, KeyValuePair<Class, Method>>> _moduleMethodMap = new Dictionary<Module, Dictionary<int, KeyValuePair<Class, Method>>>(); 

        protected BasePersistance(ICommandLine commandLine, ILog logger)
        {
            CommandLine = commandLine;
            _logger = logger;
            CoverageSession = new CoverageSession();
            _trackedMethodId = 0;
        }

        public CoverageSession CoverageSession { get; private set; }

        public void PersistModule(Module module)
        {
            if (module == null) return;
            module.Classes = module.Classes ?? new Class[0];
            if (CommandLine.MergeByHash)
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
            _moduleMethodMap[module] = new Dictionary<int, KeyValuePair<Class, Method>>();
            foreach(var @class in module.Classes)
            {
                foreach (var method in @class.Methods)
                {
                    _moduleMethodMap[module][method.MetadataToken] = new KeyValuePair<Class, Method>(@class, method);        
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

            if (CommandLine.HideSkipped == null) return;
            
            if (!CommandLine.HideSkipped.Any()) return;
            
            foreach (var skippedMethod in CommandLine.HideSkipped.OrderBy(x => x))
            {
                switch (skippedMethod)
                {
                    case SkippedMethod.File:
                        RemoveSkippedMethods(SkippedMethod.File);
                        RemoveEmptyClasses();
                        RemoveUnreferencedFiles();
                        break;
                    case SkippedMethod.Filter:
                        RemoveSkippedModules(SkippedMethod.Filter);
                        RemoveSkippedClasses(SkippedMethod.Filter);
                        break;
                    case SkippedMethod.MissingPdb:
                        RemoveSkippedModules(SkippedMethod.MissingPdb);
                        break;
                    case SkippedMethod.Attribute:
                        RemoveSkippedClasses(SkippedMethod.Attribute);
                        RemoveSkippedMethods(SkippedMethod.Attribute);
                        RemoveEmptyClasses();
                        break;
                    case SkippedMethod.AutoImplementedProperty:
                        RemoveSkippedMethods(SkippedMethod.Attribute);
                        RemoveEmptyClasses();
                        break;
                }
            }
        }

        private void RemoveSkippedModules(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) return;
            var modules = CoverageSession.Modules;
            modules = modules.Where(x => x.SkippedDueTo != skipped).ToArray();
            CoverageSession.Modules = modules;
        }

        private void RemoveSkippedClasses(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) continue;
                var classes = module.Classes.Where(x => x.SkippedDueTo != skipped).ToArray();
                module.Classes = classes;
            }
        }

        private void RemoveSkippedMethods(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) continue;
                foreach (var @class in module.Classes)
                {
                    if (@class.Methods == null) continue;
                    var methods = @class.Methods.Where(x => x.SkippedDueTo != skipped).ToArray();
                    @class.Methods = methods;
                }
            }
        }

        private void RemoveEmptyClasses()
        {
            if (CoverageSession.Modules == null) return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) continue;
                module.Classes = module.Classes.Where(@class => @class.Methods != null && @class.Methods.Any()).ToArray();
            }
        }

        private void RemoveUnreferencedFiles()
        {
            if (CoverageSession.Modules == null) return;
            foreach (var module in CoverageSession.Modules)
            {
                module.Files = (from file in module.Files ?? new File[0] 
                                from @class in module.Classes ?? new Class[0]
                                where (@class.Methods ?? new Method[0]).Where(x=>x.FileRef != null).Any(x => x.FileRef.UniqueId == file.UniqueId)
                                select file).Distinct().ToArray();
            }
        }

        protected void PopulateInstrumentedPoints()
        {
            if (CoverageSession.Modules == null) return;

            foreach (var method in from @class in
                                       (from module in CoverageSession.Modules
                                        from @class in module.Classes ?? new Class[0]
                                        select @class)
                                   where @class.Methods.Any(m => m.ShouldSerializeSkippedDueTo())
                                   where @class.Methods.All(m => m.FileRef == null)
                                   from method in @class.Methods.Where(x => !x.ShouldSerializeSkippedDueTo())
                                   select method)
            {
                method.MarkAsSkipped(SkippedMethod.Inferred);
            }

            foreach (var module in CoverageSession.Modules.Where(x => !x.ShouldSerializeSkippedDueTo()))
            {
                foreach (var @class in (module.Classes ?? new Class[0]).Where(x => !x.ShouldSerializeSkippedDueTo()))
                {

                    foreach (var method in (@class.Methods ?? new Method[0]).Where(x => !x.ShouldSerializeSkippedDueTo()))
                    {
                        var sequencePoints = method.SequencePoints ?? new SequencePoint[0];
                        var branchPoints = method.BranchPoints ?? new BranchPoint[0];

                        #region Merge branch-exits

                        // anything to merge?
                        if (sequencePoints.Length != 0 && branchPoints.Length != 0) {

                            #region Join Sequences and Branches

                            int index = 0;
                            int nextOffset = 0;
                            
                            // get first sequencePoint and prepare list for Add(branchPoint)
                            var parent = sequencePoints[index];
                            parent.BranchPoints = new List<BranchPoint>();

                            // get nextOffset
                            if (index + 1 < sequencePoints.Length) {
                                nextOffset = sequencePoints[index + 1].Offset;
                            }
                            else {
                                nextOffset = int.MaxValue;
                            }
                            
                            foreach (var branchPoint in branchPoints) {
                                
                                // while branchPoint belongs to next sequencePoint
                                // nextOffset is offset of next sequencePoint 
                                // or unreachable int.MinValue
                                while (branchPoint.Offset > nextOffset) {

                                    // increment index to next sequencePoint
                                    ++index; 

                                    // get next sequencePoint and prepare list for Add(branchPoint)
                                    parent = sequencePoints[index];
                                    parent.BranchPoints = new List<BranchPoint>();

                                    // get nextOffset
                                    if (index + 1 < sequencePoints.Length) {
                                        nextOffset = sequencePoints[index + 1].Offset;
                                    }
                                    else {
                                        nextOffset = int.MaxValue;
                                    }
                                }
                                // join BranchPoint to SequencePoint
                                parent.BranchPoints.Add(branchPoint);
                            }
    
                            #endregion
                            
                            #region Merge each Sequence Branches

                            var branchExits = new Dictionary<int, BranchPoint>();
                            foreach (var sequencePoint in sequencePoints) {
                
                                // SequencePoint visited & has branches attached?
                                if (sequencePoint.VisitCount != 0 
                                    && sequencePoint.BranchPoints != null
                                    && sequencePoint.BranchPoints.Count != 0) {
                
                                    // Merge Branches using EndOffset as branchExits key
                                    branchExits.Clear();
                                    foreach (var branchPoint in sequencePoint.BranchPoints) {
                                        if (!branchExits.ContainsKey(branchPoint.EndOffset)) {
                                            branchExits[branchPoint.EndOffset] = branchPoint; // insert branch
                                        } else {
                                            branchExits[branchPoint.EndOffset].VisitCount += branchPoint.VisitCount; // update branch
                                        }
                                    }
                
                                    // Update SequencePoint properties/attributes
                                    sequencePoint.BranchExits = 0;
                                    sequencePoint.BranchExitsVisited = 0;
                                    foreach (var branchPoint in branchExits.Values) {
                                        sequencePoint.BranchExits += 1;
                                        sequencePoint.BranchExitsVisited += branchPoint.VisitCount == 0? 0 : 1 ;
                                    }
                                }
                                sequencePoint.BranchPoints = null; // release memory
                            }
                                            
                            #endregion

                        }

                        #endregion

                        if (method.MethodPoint != null)
                        {
                            method.Visited = (method.MethodPoint.VisitCount > 0);
                        }

                        method.Summary.NumBranchPoints = branchPoints.Count();
                        method.Summary.VisitedBranchPoints = branchPoints.Count(pt => pt.VisitCount != 0);
                        method.Summary.NumSequencePoints = sequencePoints.Count();
                        method.Summary.VisitedSequencePoints = sequencePoints.Count(pt => pt.VisitCount != 0);

                        if (method.Summary.NumSequencePoints > 0)
                            method.Summary.NumBranchPoints += 1;

                        if (method.Summary.VisitedSequencePoints > 0)
                            method.Summary.VisitedBranchPoints += 1;

                        AddPoints(@class.Summary, method.Summary);
                        CalculateCoverage(method.Summary);

                        method.SequenceCoverage = method.Summary.SequenceCoverage;
                        method.BranchCoverage = method.Summary.BranchCoverage;

                        method.Summary.MinCyclomaticComplexity = method.Summary.MaxCyclomaticComplexity = Math.Max(1, method.CyclomaticComplexity);

                        if (@class.Summary.MinCyclomaticComplexity == 0)
                            @class.Summary.MinCyclomaticComplexity = method.Summary.MinCyclomaticComplexity;
                        
                        @class.Summary.MinCyclomaticComplexity = Math.Min(@class.Summary.MinCyclomaticComplexity, method.CyclomaticComplexity);
                        @class.Summary.MaxCyclomaticComplexity = Math.Max(@class.Summary.MaxCyclomaticComplexity, method.CyclomaticComplexity);
                    }

                    AddPoints(module.Summary, @class.Summary);
                    CalculateCoverage(@class.Summary);

                    if (module.Summary.MinCyclomaticComplexity == 0)
                        module.Summary.MinCyclomaticComplexity = @class.Summary.MinCyclomaticComplexity;

                    module.Summary.MinCyclomaticComplexity = Math.Min(module.Summary.MinCyclomaticComplexity, @class.Summary.MinCyclomaticComplexity);
                    module.Summary.MaxCyclomaticComplexity = Math.Max(module.Summary.MaxCyclomaticComplexity, @class.Summary.MaxCyclomaticComplexity);
                }

                AddPoints(CoverageSession.Summary, module.Summary);
                CalculateCoverage(module.Summary);

                if (CoverageSession.Summary.MinCyclomaticComplexity == 0)
                    CoverageSession.Summary.MinCyclomaticComplexity = module.Summary.MinCyclomaticComplexity;

                CoverageSession.Summary.MinCyclomaticComplexity = Math.Min(CoverageSession.Summary.MinCyclomaticComplexity, module.Summary.MinCyclomaticComplexity);
                CoverageSession.Summary.MaxCyclomaticComplexity = Math.Max(CoverageSession.Summary.MaxCyclomaticComplexity, module.Summary.MaxCyclomaticComplexity);
            }

            CalculateCoverage(CoverageSession.Summary);
        }

        private static void CalculateCoverage(Summary summary)
        {
            if (summary.NumSequencePoints > 0)
                summary.SequenceCoverage = Math.Round((summary.VisitedSequencePoints*100m)/summary.NumSequencePoints, 2);

            if (summary.NumBranchPoints > 0)
                summary.BranchCoverage = Math.Round((summary.VisitedBranchPoints*100m)/summary.NumBranchPoints, 2);
        }

        private static void AddPoints(Summary parent, Summary child)
        {
            parent.NumBranchPoints += child.NumBranchPoints;
            parent.VisitedBranchPoints += child.VisitedBranchPoints;
            parent.NumSequencePoints += child.NumSequencePoints;
            parent.VisitedSequencePoints += child.VisitedSequencePoints;
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
            if (!_moduleMethodMap[module].ContainsKey(functionToken)) return null;
            var pair = _moduleMethodMap[module][functionToken];
            @class = pair.Key;
            return pair.Value;
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
                {
                    if (!InstrumentationPoint.AddVisitCount(spid, _trackedMethodId))
                    {
                        _logger.DebugFormat("Failed to add a visit to {0} with tracking method {1}. Max point count is {2}", 
                            spid, _trackedMethodId, InstrumentationPoint.Count);
                    }
                }
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