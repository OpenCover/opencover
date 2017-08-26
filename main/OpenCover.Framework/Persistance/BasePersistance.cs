using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;
using OpenCover.Framework.Utility;
using log4net;

namespace OpenCover.Framework.Persistance
{
    /// <summary>
    /// A basic layer that aggregates the data
    /// </summary>
    public abstract class BasePersistance : IPersistance
    {
        private static readonly object Protection = new object();

        /// <summary>
        /// Provides subclasses access to the command line object
        /// </summary>
        protected readonly ICommandLine CommandLine;
        private readonly ILog _logger;
        private uint _trackedMethodId;
        private readonly Dictionary<Module, Dictionary<int, KeyValuePair<Class, Method>>> _moduleMethodMap = new Dictionary<Module, Dictionary<int, KeyValuePair<Class, Method>>>();

        private static readonly ILog DebugLogger = LogManager.GetLogger("DebugLogger");

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="logger"></param>
        protected BasePersistance(ICommandLine commandLine, ILog logger)
        {
            CommandLine = commandLine;
            _logger = logger ?? DebugLogger;
            CoverageSession = new CoverageSession();
            _trackedMethodId = 0;
        }

        /// <summary>
        /// A coverage session
        /// </summary>
        public CoverageSession CoverageSession { get; private set; }

        /// <summary>
        /// Add the <see cref="Module"/> to the current session
        /// </summary>
        /// <param name="module"></param>
        public void PersistModule(Module module)
        {
            if (module == null) 
                return;
            module.Classes = module.Classes ?? new Class[0];
            if (CommandLine.MergeByHash)
            {
                lock (Protection)
                {
                    var modules = CoverageSession.Modules ?? new Module[0];
                    lock (Protection)
                    {
                        var existingModule = modules.FirstOrDefault(x => x.ModuleHash == module.ModuleHash);
                        if (existingModule != null)
                        {
                            if (
                                !existingModule.Aliases.Any(
                                    x => x.Equals(module.ModulePath, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                existingModule.Aliases.Add(module.ModulePath);
                            }
                            return;
                        }
                    }
                }
            }

            _moduleMethodMap[module] = new Dictionary<int, KeyValuePair<Class, Method>>();
            BuildMethodMapForModule(module);
            var list = new List<Module>(CoverageSession.Modules ?? new Module[0]) { module };
            CoverageSession.Modules = list.ToArray();
        }

        /// <summary>
        /// Clear the current coverage session data
        /// </summary>
        protected void ClearCoverageSession()
        {
            _moduleMethodMap.Clear();
            CoverageSession = new CoverageSession();
            InstrumentationPoint.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        protected void ReassignCoverageSession(CoverageSession session)
        {
            _moduleMethodMap.Clear();
            CoverageSession = session;
            CoverageSession.Summary = new Summary();
            foreach (var module in CoverageSession.Modules)
            {
                BuildMethodMapForModule(module);
                module.Summary = new Summary();
                foreach (var @class in module.Classes)
                {
                    @class.Summary = new Summary();
                    foreach (var method in @class.Methods)
                    {
                        method.Summary = new Summary();
                        if (method.SequencePoints.Any() && method.SequencePoints[0].Offset == method.MethodPoint.Offset)
                        {
                            var point = new[] { method.SequencePoints[0], (SequencePoint)method.MethodPoint }
                                .OrderBy(x => x.OrigSequencePoint)
                                .First();

                            method.MethodPoint = point;
                            method.SequencePoints[0] = point;
                        }
                    }
                }
            }

            InstrumentationPoint.ResetAfterLoading();
            File.ResetAfterLoading();
        }

        private void BuildMethodMapForModule(Module module)
        {
            _moduleMethodMap[module] = new Dictionary<int, KeyValuePair<Class, Method>>();
            foreach (var @class in module.Classes)
            {
                foreach (var method in @class.Methods)
                {
                    _moduleMethodMap[module][method.MetadataToken] = new KeyValuePair<Class, Method>(@class, method);
                }
            }
        }

        /// <summary>
        /// Is the module being tracked
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        public bool IsTracking(string modulePath)
        {
            lock (Protection) {
                return CoverageSession.Modules.Any(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)) &&
                        !x.ShouldSerializeSkippedDueTo());
            }
        }

        /// <summary>
        /// we are done and the data needs one last clean up
        /// </summary>
        public virtual void Commit()
        {
            if (CoverageSession.Modules == null) 
                return;
            MarkSkippedMethods();
            TransformSequences();
            CalculateCoverage();
            if (CommandLine.HideSkipped == null || !CommandLine.HideSkipped.Any()) 
                return;
            foreach (var skippedMethod in CommandLine.HideSkipped.OrderBy(x => x))
                ProcessSkippedAction(skippedMethod);
        }

        private void ProcessSkippedAction(SkippedMethod skippedMethod)
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
                case SkippedMethod.FolderExclusion:
                    RemoveSkippedModules(SkippedMethod.FolderExclusion);
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

        private void RemoveSkippedModules(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) 
                return;
            var modules = CoverageSession.Modules;
            modules = modules.Where(x => x.SkippedDueTo != skipped).ToArray();
            CoverageSession.Modules = modules;
        }

        private void RemoveSkippedClasses(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) 
                return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) 
                    continue;
                var classes = module.Classes.Where(x => x.SkippedDueTo != skipped).ToArray();
                module.Classes = classes;
            }
        }

        private void RemoveSkippedMethods(SkippedMethod skipped)
        {
            if (CoverageSession.Modules == null) 
                return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) 
                    continue;
                foreach (var @class in module.Classes)
                {
                    if (@class.Methods == null) 
                        continue;
                    var methods = @class.Methods.Where(x => x.SkippedDueTo != skipped).ToArray();
                    @class.Methods = methods;
                }
            }
        }

        private void RemoveEmptyClasses()
        {
            if (CoverageSession.Modules == null) 
                return;
            foreach (var module in CoverageSession.Modules)
            {
                if (module.Classes == null) 
                    continue;
                module.Classes = module.Classes.Where(@class => @class.Methods != null && @class.Methods.Any()).ToArray();
            }
        }

        private void RemoveUnreferencedFiles()
        {
            if (CoverageSession.Modules == null) 
                return;
            foreach (var module in CoverageSession.Modules)
            {
                module.Files = (from file in module.Files ?? new File[0]
                                from @class in module.Classes ?? new Class[0]
                                where (@class.Methods ?? new Method[0]).Where(x=>x.FileRef != null).Any(x => x.FileRefUniqueId == file.UniqueId)
                                select file).Distinct().ToArray();
            }
        }

        private void MarkSkippedMethods()
        {
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
        }

        private void CalculateCoverage()
        {
            foreach (var module in CoverageSession.Modules.Where(x => x != null && !x.ShouldSerializeSkippedDueTo()))
            {
                foreach (var @class in (module.Classes ?? new Class[0]).Where(x => x != null && !x.ShouldSerializeSkippedDueTo()))
                {
                    foreach (var method in (@class.Methods ?? new Method[0]).Where(x => x != null && !x.ShouldSerializeSkippedDueTo()))
                        ProcessMethodData(method, @class);

                    ProcessClassData(@class, module);
                }
                ProcessModuleData(module);
            }
            CalculateCoverage(CoverageSession.Summary);
        }

        private void ProcessModuleData(Module module)
        {
            AddPoints(CoverageSession.Summary, module.Summary);
            CalculateCoverage(module.Summary);

            if (CoverageSession.Summary.MinCrapScore == 0)
            {
                CoverageSession.Summary.MinCrapScore = module.Summary.MinCrapScore;
            }
            CoverageSession.Summary.MinCrapScore = Math.Min(CoverageSession.Summary.MinCrapScore, module.Summary.MinCrapScore);
            CoverageSession.Summary.MaxCrapScore = Math.Max(CoverageSession.Summary.MaxCrapScore, module.Summary.MaxCrapScore);

            if (CoverageSession.Summary.MinCyclomaticComplexity == 0)
                CoverageSession.Summary.MinCyclomaticComplexity = module.Summary.MinCyclomaticComplexity;

            CoverageSession.Summary.MinCyclomaticComplexity = Math.Min(CoverageSession.Summary.MinCyclomaticComplexity,
                module.Summary.MinCyclomaticComplexity);
            CoverageSession.Summary.MaxCyclomaticComplexity = Math.Max(CoverageSession.Summary.MaxCyclomaticComplexity,
                module.Summary.MaxCyclomaticComplexity);
        }

        private static void ProcessClassData(Class @class, Module module)
        {
            @class.Summary.NumClasses = (@class.Summary.NumMethods > 0) ? 1 : 0;
            @class.Summary.VisitedClasses = (@class.Summary.VisitedMethods > 0) ? 1 : 0;

            AddPoints(module.Summary, @class.Summary);
            CalculateCoverage(@class.Summary);

            if (module.Summary.MinCrapScore == 0)
            {
                module.Summary.MinCrapScore = @class.Summary.MinCrapScore;
            }
            module.Summary.MinCrapScore = Math.Min(module.Summary.MinCrapScore, @class.Summary.MinCrapScore);
            module.Summary.MaxCrapScore = Math.Max(module.Summary.MaxCrapScore, @class.Summary.MaxCrapScore);

            if (module.Summary.MinCyclomaticComplexity == 0)
                module.Summary.MinCyclomaticComplexity = @class.Summary.MinCyclomaticComplexity;

            module.Summary.MinCyclomaticComplexity = Math.Min(module.Summary.MinCyclomaticComplexity,
                @class.Summary.MinCyclomaticComplexity);
            module.Summary.MaxCyclomaticComplexity = Math.Max(module.Summary.MaxCyclomaticComplexity,
                @class.Summary.MaxCyclomaticComplexity);
        }

        private static void ProcessMethodData(Method method, Class @class)
        {
            if (method.MethodPoint != null)
            {
                method.Visited = (method.MethodPoint.VisitCount > 0);
            }

            method.Summary.NumBranchPoints = method.BranchPoints.Length;
            method.Summary.VisitedBranchPoints = method.BranchPoints.Count(pt => pt.VisitCount != 0);
            method.Summary.NumSequencePoints = method.SequencePoints.Length;
            method.Summary.VisitedSequencePoints = method.SequencePoints.Count(pt => pt.VisitCount != 0);

            if (method.Summary.NumSequencePoints > 0)
                method.Summary.NumBranchPoints += 1;

            if (method.Summary.VisitedSequencePoints > 0)
                method.Summary.VisitedBranchPoints += 1;

            if (method.FileRef != null)
            {
                method.Summary.NumMethods = 1;
                method.Summary.VisitedMethods = (method.Visited) ? 1 : 0;
            }

            AddPoints(@class.Summary, method.Summary);
            CalculateCoverage(method.Summary);

            method.SequenceCoverage = method.Summary.SequenceCoverage;
            method.BranchCoverage = method.Summary.BranchCoverage;

            CalculateCrapScore(method, @class);

            CalculateNPathComplexity(method);

            CalculateCyclomaticComplexity(method, @class);
        }

        private static void CalculateCrapScore(Method method, Class @class)
        {
            method.CrapScore = Math.Round((decimal) Math.Pow(method.CyclomaticComplexity, 2) *
                                          (decimal) Math.Pow(1.0 - (double) (method.SequenceCoverage / (decimal) 100.0), 3.0) +
                                          method.CyclomaticComplexity, 
                                          2);

            // TODO: is 0 a possible crap score?
            method.Summary.MinCrapScore = Math.Max(0, method.CrapScore);
            method.Summary.MaxCrapScore = method.Summary.MinCrapScore;

            if (@class.Summary.MinCrapScore == 0)
            {
                @class.Summary.MinCrapScore = method.Summary.MinCrapScore;
            }

            @class.Summary.MinCrapScore = Math.Min(@class.Summary.MinCrapScore, method.CrapScore);
            @class.Summary.MaxCrapScore = Math.Max(@class.Summary.MaxCrapScore, method.CrapScore);
        }

        private static void CalculateCyclomaticComplexity(Method method, Class @class)
        {
            method.Summary.MinCyclomaticComplexity = Math.Max(1, method.CyclomaticComplexity);
            method.Summary.MaxCyclomaticComplexity = method.Summary.MinCyclomaticComplexity;

            if (@class.Summary.MinCyclomaticComplexity == 0)
                @class.Summary.MinCyclomaticComplexity = method.Summary.MinCyclomaticComplexity;

            @class.Summary.MinCyclomaticComplexity = Math.Min(@class.Summary.MinCyclomaticComplexity,
                method.CyclomaticComplexity);
            @class.Summary.MaxCyclomaticComplexity = Math.Max(@class.Summary.MaxCyclomaticComplexity,
                method.CyclomaticComplexity);
        }

        private static void CalculateNPathComplexity(Method method)
        {
            method.NPathComplexity = 0;
            var nPaths = new Dictionary<int, int>();
            if (method.BranchPoints.Any())
            {
                foreach (var bp in method.BranchPoints.Where(b => b != null))
                {
                    if (nPaths.ContainsKey(bp.Offset))
                    {
                        nPaths[bp.Offset] += 1;
                    }
                    else
                    {
                        nPaths.Add(bp.Offset, 1);
                    }
                }
            }
            foreach (var branches in nPaths.Values)
            {
                if (method.NPathComplexity == 0)
                {
                    method.NPathComplexity = branches;
                }
                else
                {
                    try
                    {
                        method.NPathComplexity = checked(method.NPathComplexity * branches);
                    }
                    catch (OverflowException)
                    {
                        method.NPathComplexity = int.MaxValue;
                        break;
                    }
                }
            }
        }

        private static void MapFileReferences(IEnumerable<IDocumentReference> points, IDictionary<string, uint> filesDictionary)
        {
            foreach (var pt in points.Where(p => p.FileId == 0))
            {
                filesDictionary.TryGetValue(pt.Document ?? "", out uint fileid);
                pt.FileId = fileid;
                // clear document if FileId is found
                pt.Document = pt.FileId != 0 ? null : pt.Document;
            }
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

            parent.NumClasses += child.NumClasses;
            parent.VisitedClasses += child.VisitedClasses;
            parent.NumMethods += child.NumMethods;
            parent.VisitedMethods += child.VisitedMethods;
        }

        /// <summary>
        /// Get the sequence points for a function
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="functionToken"></param>
        /// <param name="sequencePoints"></param>
        /// <returns></returns>
        public bool GetSequencePointsForFunction(string modulePath, int functionToken, out InstrumentationPoint[] sequencePoints)
        {
            sequencePoints = new InstrumentationPoint[0];
            var method = GetMethod(modulePath, functionToken, out Class @class);
            if (method != null && method.SequencePoints.Any())
            {
                System.Diagnostics.Debug.WriteLine("Getting Sequence points for {0}({1})", method.FullName, method.MetadataToken);
                var points = new List<InstrumentationPoint>();
                if (!(method.MethodPoint is SequencePoint))
                    points.Add(method.MethodPoint);
                points.AddRange(method.SequencePoints);
                sequencePoints = points.ToArray();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the branch ponts for a function
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="functionToken"></param>
        /// <param name="branchPoints"></param>
        /// <returns></returns>
        public bool GetBranchPointsForFunction(string modulePath, int functionToken, out BranchPoint[] branchPoints)
        {
            branchPoints = new BranchPoint[0];
            var method = GetMethod(modulePath, functionToken, out Class @class);
            if (method != null && method.BranchPoints.Any())
            {
                System.Diagnostics.Debug.WriteLine("Getting Branch points for {0}({1})", method.FullName, method.MetadataToken);
                branchPoints = method.BranchPoints.ToArray();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get <see cref="Method"/> data for a function
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="functionToken"></param>
        /// <param name="class"></param>
        /// <returns></returns>
        private Method GetMethod(string modulePath, int functionToken, out Class @class)
        {
            @class = null;
            lock (Protection)
            {
                var module = CoverageSession.Modules
                    .FirstOrDefault(x => x.Aliases.Any(path => path.Equals(modulePath, StringComparison.InvariantCultureIgnoreCase)));
                if (module == null)
                    return null;
                if (!_moduleMethodMap[module].ContainsKey(functionToken)) 
                    return null;
                var pair = _moduleMethodMap[module][functionToken];
                @class = pair.Key;
                return pair.Value;
            }
        }

        /// <summary>
        /// Get the class name of a function
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="functionToken"></param>
        /// <returns></returns>
        public string GetClassFullName(string modulePath, int functionToken)
        {
            GetMethod(modulePath, functionToken, out Class @class);
            return @class?.FullName;
        }

        /// <summary>
        /// Save the visit data to the session model
        /// </summary>
        /// <param name="data"></param>
        public void SaveVisitData(byte[] data)
        {
            var nCount = BitConverter.ToUInt32(data, 0);
            if (nCount > (data.Count()/4) - 1)
            {
                _logger.ErrorFormat("Failed to process points as count ({0}) exceeded available buffer size ({1})",
                    nCount, (data.Count()/4) - 1);
                return;
            }
            for (int i = 0, idx = 4; i < nCount; i++, idx += 4)
            {
                var spid = BitConverter.ToUInt32(data, idx);
                if (spid < (uint)MSG_IdType.IT_MethodEnter)
                {
                    if (!InstrumentationPoint.AddVisitCount(spid, _trackedMethodId, 1))
                    {
                        _logger.ErrorFormat("Failed to add a visit to {0} with tracking method {1}. Max point count is {2}",
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

        /// <summary>
        /// determine if the method (test method) should be tracked
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <param name="functionToken"></param>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool GetTrackingMethod(string modulePath, string assemblyName, int functionToken, out uint uniqueId)
        {
            lock (Protection)
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
            }
            return false;
        }

        private void TransformSequences() {

            var sessionModulesQuery = CoverageSession.Modules.Where(module => module != null && !module.ShouldSerializeSkippedDueTo());
            foreach(var module in sessionModulesQuery) {

                var moduleClassesQuery = (module.Classes ?? new Class[0]).Where(x => x != null && !x.ShouldSerializeSkippedDueTo());
                var moduleMethodsQuery = moduleClassesQuery.SelectMany(@class => (@class.Methods ?? new Method[0])).Where(x => x != null && !x.ShouldSerializeSkippedDueTo());
                var methods = moduleMethodsQuery.ToArray();

                if (methods.Any()) {
                    var sourceRepository = new SourceRepository(); // module sources
                    // keep transformations in this order!
                    TransformSequences_Initialize (methods);
                    TransformSequences_JoinWithBranches (methods);
                    TransformSequences_AddSources (module.Files, methods, sourceRepository);
                    TransformSequences_RemoveCompilerGeneratedBranches  (methods, sourceRepository, module.ModuleTime);
                    TransformSequences_RemoveFalsePositiveUnvisited (methods, sourceRepository, module.ModuleTime);
                    TransformSequences_ReduceBranches (methods); // last
                }
            }
        }

        static void TransformSequences_Initialize (IEnumerable<Method> methods)
        {
            foreach (var method in methods) {
                // No sequences in method, but branches present? => remove branches
                if (method.SequencePoints.Length == 0 && method.BranchPoints.Length != 0) {
                    method.BranchPoints = new BranchPoint[0];
                }
            }
        }

        private static void TransformSequences_JoinWithBranches (IEnumerable<Method> methods)
        {
            foreach (var method in methods) {

                if (method.SequencePoints.Length != 0 && method.BranchPoints.Length != 0) {
                    // Quick match branches to sequence using SP&BP sort order by IL offset
                    // SP & BP are sorted by offset and code below expect both SP & BP to be sorted by offset
                    // ATTN: Sorted again to prevent future bugs if order of SP & BP is changed!
                    method.SequencePoints = method.SequencePoints.OrderBy(sp => sp.Offset).ToArray();
                    method.BranchPoints = method.BranchPoints.OrderBy(bp => bp.Offset).ToArray();
                    // Use stack because Stack.Pop is constant time
                    var branchStack = new Stack<BranchPoint>(method.BranchPoints);
                    // Join offset matching BranchPoints with SequencePoint "parent"
                    // Exclude all branches where BranchPoint.Offset < first method.SequencePoints.Offset
                    // Reverse() starts loop from highest offset to lowest
                    foreach (SequencePoint spParent in method.SequencePoints.Reverse()) {
                        // create branchPoints "child" list
                        spParent.BranchPoints = new List<BranchPoint>();
                        // if BranchPoint.Offset is >= SequencePoint.Offset
                        // then move BranchPoint from stack to "child" list (Pop/Add)
                        while (branchStack.Count != 0 && branchStack.Peek().Offset >= spParent.Offset) {
                            spParent.BranchPoints.Add(branchStack.Pop());
                        }
                    }
                }
            }
        }

        private static void TransformSequences_AddSources (IEnumerable<File> files, IEnumerable<Method> methods, IDictionary<uint, CodeCoverageStringTextSource> sourceRepository)
        {
            if (files == null || !files.Any()) 
                return;

            // Dictionary with stored source file names per module
            var filesDictionary = new Dictionary<string, uint>();

            foreach (var file in files.
                Where (file => !String.IsNullOrWhiteSpace(file.FullPath)
                            && !filesDictionary.ContainsKey(file.FullPath)))
            {
                var source = CodeCoverageStringTextSource.GetSource(file.FullPath); // never reurns null
                if (source.FileType == FileType.CSharp) 
                    sourceRepository.Add (file.UniqueId, source);
                filesDictionary.Add(file.FullPath, file.UniqueId);
            }
    
            foreach (var method in methods) {
                if (method.SequencePoints.Length != 0)
                    MapFileReferences(method.SequencePoints, filesDictionary);
                if (method.BranchPoints.Length != 0)
                    MapFileReferences(method.BranchPoints, filesDictionary);
            }
        }

        private static void TransformSequences_RemoveCompilerGeneratedBranches (IEnumerable<Method> methods, SourceRepository sourceRepository, DateTime moduleTime)
        {
            foreach (var method in methods
                     .Where (m => m.FileRefUniqueId != 0
                             && m.SequencePoints.Length != 0)) {
            
                // Get method source if availabile 
                var source = sourceRepository.GetCodeCoverageStringTextSource(method.FileRefUniqueId);
                if (source != null) {

                    if (source.IsChanged (moduleTime)) {
                        ("Source file is modified: " + source.FilePath).InformUser();
                        return;
                    }

                    if (!TransformSequences_RemoveCompilerGeneratedBranches (method, source)) {
                        return; // empty sequence found -> source.IsChanged (failed access to file-times)
                    }
                }
            }
        }

        private static bool TransformSequences_RemoveCompilerGeneratedBranches (Method method, CodeCoverageStringTextSource source)
        {
            // Do we have C# source?
            if (source.FileType == FileType.CSharp) {

                if (source.FileFound) {

                    // initialize offset with unreachable values
                    long startOffset = long.MinValue;
                    long finalOffset = long.MaxValue;

                    if (!method.IsGenerated) {
                        // fill offsets with values
                        TransformSequences_RemoveCompilerGeneratedBranches(method, source, ref startOffset, ref finalOffset);
                    }

                    if (!TransformSequences_RemoveCompilerGeneratedBranches (method, source, startOffset, finalOffset)) {
                        return false; // return error/failure to caller
                    }

                } else {

                    // Do as much possible without source
                    // This will remove generated branches within "{", "}" and "in" (single-line SequencePoints)
                    // but cannot remove Code Contract ccrewite generated branches
                    foreach (var sp in method.SequencePoints) {
                        if (sp.BranchPoints.Count != 0
                            && sp.StartLine == sp.EndLine
                            && sp.EndColumn - sp.StartColumn <= 2) {

                            // Zero, one or two character sequence point should not contain branches
                            // Never found 0 character sequencePoint
                            // Never found 1 character sequencePoint except "{" and "}"
                            // Never found 2 character sequencePoint except "in" keyword
                            // Afaik, c# cannot express branch condition in one or two characters of source code
                            // Keyword "do" does not generate SequencePoint 
                            sp.BranchPoints = new List<BranchPoint>();
                        }
                    }
                }
            }
            return true;
        }

        private static void TransformSequences_RemoveCompilerGeneratedBranches(Method method, CodeCoverageStringTextSource source, ref long startOffset, ref long finalOffset)
        {
            // order SequencePoints by source order (Line/Column)
            var sourceLineOrderedSps = method.SequencePoints.OrderBy(sp => sp.StartLine).ThenBy(sp => sp.StartColumn).Where(sp => sp.FileId == method.FileRefUniqueId).ToArray();

            // get "{" if on first two positions
            for (int index = 0; index < Math.Min (2, sourceLineOrderedSps.Length); index++) {
                if (source.GetText(sourceLineOrderedSps[0]) == "{") {
                    startOffset = sourceLineOrderedSps[0].Offset;
                    break;
                }
            }
            // get "}" if on last position
            if (source.GetText(sourceLineOrderedSps.Last()) == "}") {
                finalOffset = sourceLineOrderedSps.Last().Offset;
            }
        }

        // Compiled for speed, treat as .Singleline for multiline SequencePoint, do not waste time to capture Groups (speed)
        private static readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
        // "Contract" and "." and "Requires/Ensures" can be separated by spaces and newlines (valid c# syntax)!
        private static readonly Regex contractRegex = new Regex(@"^Contract\s*\.\s*(Requi|Ensu)res", regexOptions);

        private static bool TransformSequences_RemoveCompilerGeneratedBranches (Method method, CodeCoverageStringTextSource source, long startOffset, long finalOffset) {

            // foreach sequence point
            foreach (var sp in method.SequencePoints
                     .Where (sp => sp.FileId == method.FileRefUniqueId 
                             && sp.BranchPoints.Count != 0)) {

                if (sp.Offset <= startOffset || sp.Offset >= finalOffset) {
                    // doRemoveBranches where .Offset <= startOffset"{" or finalOffset"}" <= .Offset
                    // this will exclude "{" and "}" compiler generated branches and majority of ccrewrite Code-Contract's
                    sp.BranchPoints = new List<BranchPoint>();

                } else { // branches not removed
                    // check for other options by reading SequencePoint source
                    var text = source.GetText(sp); // text is never null
                    if (text.Length == 0) {
                        ("Empty sequence-point at line: " + sp.StartLine + " column: " + sp.StartColumn).InformUser();
                        ("Source file: " + source.FilePath).InformUser();
                        return false; // signal error to caller's caller loop (break)
                    }
                    // Contract.Requires/Ensures is occasionally left inside method offset
                    // Quick check for "C" before using Regex
                    // Use Regex here! "Contract" and "." and "Requires/Ensures"
                    // can be separated by spaces and newlines
                    if (text[0] == 'C' && contractRegex.IsMatch(text)) {
                        sp.BranchPoints = new List<BranchPoint>();
                    } 
                    // "in" keyword?
                    if (text == "in") {
                        // Remove generated ::MoveNext branches within "in" keyword
                        // Not always removed in CecilSymbolManager (enumerated KeyValuePair)
                        sp.BranchPoints = new List<BranchPoint>();
                    }
                }
            }
            return true;
        }

        private static void TransformSequences_RemoveFalsePositiveUnvisited (IEnumerable<Method> methods, SourceRepository sourceRepository, DateTime moduleTime)
        {
            var sequencePointsSet = new HashSet<SequencePoint>(new SequencePointComparer());
            var toRemoveMethodSequencePoint = new List<Tuple<Method, SequencePoint>>();

            // Initialise sequencePointsSet
            TransformSequences_RemoveFalsePositiveUnvisited(methods, sequencePointsSet);

            // Check generated methods
            foreach (var method in methods
                     .Where (m => m.FileRefUniqueId != 0
                             && m.SequencePoints.Length != 0
                             && m.IsGenerated)) {

                // Select duplicate and false-positive unvisited sequence points
                // (Sequence point duplicated by generated method and left unvisited)
                foreach (var sp in method.SequencePoints
                         .Where (sp => sp.FileId == method.FileRefUniqueId
                                 && sp.VisitCount == 0) ) {
    
                    if (sequencePointsSet.Contains(sp)) {
                        // Unvisited duplicate found, add to remove list
                        toRemoveMethodSequencePoint.Add (new Tuple<Method, SequencePoint>(method, sp));
                    }
                }
    
                // Get method source if availabile 
                var source = sourceRepository.GetCodeCoverageStringTextSource(method.FileRefUniqueId);
                if (source != null && !source.IsChanged(moduleTime)) {
                    TransformSequences_RemoveFalsePositiveUnvisited (method, source, toRemoveMethodSequencePoint);
                }

            }

            // Remove selected SequencePoints
            foreach (var tuple in toRemoveMethodSequencePoint) {
                tuple.Item1.SequencePoints = tuple.Item1.SequencePoints.Where(sp => sp != tuple.Item2).ToArray();
            }
        }

        private static void TransformSequences_RemoveFalsePositiveUnvisited(IEnumerable<Method> methods, ISet<SequencePoint> sequencePointsSet)
        {
            // From Methods with Source and visited SequencePoints
            var sequencePointsQuery = methods.Where(m => m.FileRefUniqueId != 0 && m.SequencePoints.Length != 0).SelectMany(m => m.SequencePoints).Where(sp => sp.FileId != 0 && sp.VisitCount != 0);
            // Add unique visited SequencePoints to HashSet
            foreach (var sp in sequencePointsQuery) {
                if (!sequencePointsSet.Contains(sp)) {
                    sequencePointsSet.Add(sp);
                }
            }
        }

        private static void TransformSequences_RemoveFalsePositiveUnvisited (Method method, CodeCoverageStringTextSource source, ICollection<Tuple<Method, SequencePoint>> toRemoveMethodSequencePoint)
        {
            // Select false unvisited right-curly-braces at generated "MoveNext" method
            // (Curly braces moved to generated "MoveNext" method and left unvisited)
            // Source is required here to identify curly braces
            if (method.CallName == "MoveNext" && source.FileFound && source.FileType == FileType.CSharp) {

                int countDown = 2; // remove up to two last right-curly-braces
                foreach (var sp in method.SequencePoints.Reverse()) {
                    if (sp.FileId == method.FileRefUniqueId
                        && sp.IsSingleCharSequencePoint
                        && sp.VisitCount == 0) { // unvisited only

                        if (countDown > 0) {
                            if (source.GetText(sp) == "}") {
                                toRemoveMethodSequencePoint.Add (new Tuple<Method, SequencePoint>(method, sp));
                                countDown -= 1;
                            }
                        }
                        else {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes reduced SequencePoint branch coverage  
        /// by finding common exit offset (switch/case)
        /// </summary>
        /// <param name="methods"></param>
        private static void TransformSequences_ReduceBranches (IEnumerable<Method> methods)
        {
            foreach (var method in methods) {

                // Collection of validBranchPoints (child/connected to parent SequencePoint)
                var validBranchPoints = new List<BranchPoint>();
                var branchExits = new Dictionary<int, BranchPoint>();
                foreach (var sp in method.SequencePoints) {
                    // SequencePoint has branches attached?
                    if (sp.BranchPoints.Count != 0) {
                        // Merge sp.BranchPoints using EndOffset as branchExits key
                        branchExits.Clear();
                        foreach (var branchPoint in sp.BranchPoints) {
                            if (!branchExits.ContainsKey(branchPoint.EndOffset)) {
                                branchExits[branchPoint.EndOffset] = branchPoint;
                                // insert branch
                            } else {
                                branchExits[branchPoint.EndOffset].VisitCount += branchPoint.VisitCount;
                                // update branch
                            }
                        }
                        // Update SequencePoint counters
                        sp.BranchExitsCount = 0;
                        sp.BranchExitsVisit = 0;
                        foreach (var branchPoint in branchExits.Values) {
                            sp.BranchExitsCount += 1;
                            sp.BranchExitsVisit += branchPoint.VisitCount == 0 ? 0 : 1;
                        }
                        // Add to validBranchPoints
                        validBranchPoints.AddRange(sp.BranchPoints);
                        sp.BranchPoints = new List<BranchPoint>();
                    }
                }
                // Replace original method branchPoints with valid (filtered and joined) branches.
                // Order is Required by FilePersistanceTest because it does not sets .Offset.
                // (Order by UniqueSequencePoint is equal to order by .Offset when .Offset is set)
                method.BranchPoints = validBranchPoints.OrderBy(bp => bp.UniqueSequencePoint).ToArray();

            }
        }

    }
}
