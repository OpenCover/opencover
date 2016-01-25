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
                        if (method.SequencePoints != null && method.SequencePoints.Any() && method.SequencePoints[0].Offset == method.MethodPoint.Offset)
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
                                where (@class.Methods ?? new Method[0]).Where(x=>x.FileRef != null).Any(x => x.FileRef.UniqueId == file.UniqueId)
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

            method.Summary.NumBranchPoints = method.BranchPoints == null ? 0 : method.BranchPoints.Count();
            method.Summary.VisitedBranchPoints = method.BranchPoints == null
                ? 0
                : method.BranchPoints.Count(pt => pt.VisitCount != 0);
            method.Summary.NumSequencePoints = method.SequencePoints == null ? 0 : method.SequencePoints.Count();
            method.Summary.VisitedSequencePoints = method.SequencePoints == null
                ? 0
                : method.SequencePoints.Count(pt => pt.VisitCount != 0);

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

            CalculateNPathComplexity(method);

            CalculateCyclomaticComplexity(method, @class);
        }

        private static void CalculateCyclomaticComplexity(Method method, Class @class)
        {
            method.Summary.MinCyclomaticComplexity =
                method.Summary.MaxCyclomaticComplexity = Math.Max(1, method.CyclomaticComplexity);

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
            if (method.BranchPoints != null && method.BranchPoints.Length != 0)
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
                    method.NPathComplexity *= branches;
                }
            }
        }

        private static void MapFileReferences(IEnumerable<IDocumentReference> points, IDictionary<string, uint> filesDictionary)
        {
            foreach (var pt in points.Where(p => p.FileId == 0))
            {
                uint fileid;
                filesDictionary.TryGetValue(pt.Document ?? "", out fileid);
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
            Class @class;
            var method = GetMethod(modulePath, functionToken, out @class);
            if (method !=null && method.SequencePoints != null)
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
            Class @class;
            var method = GetMethod(modulePath, functionToken, out @class);
            if (method != null && method.BranchPoints != null)
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
            //c = null;
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
            Class @class;
            GetMethod(modulePath, functionToken, out @class);
            return @class != null ? @class.FullName : null;
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
                #region Cleanup
                // remove nulls
                if (method.SequencePoints == null)
                    method.SequencePoints = new SequencePoint[0];
                if (method.BranchPoints == null)
                    method.BranchPoints = new BranchPoint[0];
                // No sequences in method, but branches present? => remove branches
                if (method.SequencePoints.Length == 0 && method.BranchPoints.Length != 0) {
                    method.BranchPoints = new BranchPoint[0];
                }
                #endregion
            }
        }

        private static void TransformSequences_AddSources (IList<File> files, IEnumerable<Method> methods, IDictionary<uint, CodeCoverageStringTextSource> sourceRepository)
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
                #region Add file references
                if (method.SequencePoints.Length != 0)
                    MapFileReferences(method.SequencePoints, filesDictionary);
                if (method.BranchPoints.Length != 0)
                    MapFileReferences(method.BranchPoints, filesDictionary);
                #endregion
            }
        }

        private static void TransformSequences_JoinWithBranches (IEnumerable<Method> methods)
        {
            foreach (var method in methods) {
                #region Join BranchPoints children to SequencePoint parent
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
                #endregion
            }
        }

        // Compiled for speed, treat as .Singleline for multiline SequencePoint, do not waste time to capture Groups (speed)
        private static readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
        // "Contract" and "." and "Requires/Ensures" can be separated by spaces and newlines (valid c# syntax)!
        private static readonly Regex ContractRegex = new Regex(@"^Contract\s*\.\s*(Requi|Ensu)res", regexOptions);

        private static void TransformSequences_RemoveCompilerGeneratedBranches (IEnumerable<Method> methods, SourceRepository sourceRepository, DateTime moduleTime)
        {
            foreach (var method in methods) {
            
                if (method == null
                    || method.FileRef == null
                    || method.FileRef.UniqueId == 0
                    || method.SequencePoints == null
                    || method.SequencePoints.Length == 0
                   ) {
                    continue;
                }

                // Get method source if availabile 
                var source = sourceRepository.GetCodeCoverageStringTextSource(method.FileRef.UniqueId);

                // Do we have C# source?
                if (source != null 
                    && source.FileFound 
                    && source.FileType == FileType.CSharp ) {

                    if (source.FileTime != DateTime.MinValue 
                        && moduleTime != DateTime.MinValue 
                        && source.FileTime > moduleTime) {
                        ("Source file is modified: " + source.FilePath).InformUser();
                        return;
                    }

                    #region Use line/col-sorted SequencePoint's offset and content to Remove Compiler Generated Branches

                    // initialize offset with unreachable values
                    long startOffset = long.MinValue;
                    long finalOffset = long.MaxValue;

                    if (!method.IsGenerated)
                    {
                        // order SequencePoints by source order (Line/Column)
                        var sourceLineOrderedSps = method.SequencePoints.OrderBy(sp => sp.StartLine).ThenBy(sp => sp.StartColumn).Where(sp => sp.FileId == method.FileRef.UniqueId).ToArray();
    
                        // find getter/setter/static-method "{" offset
                        if (sourceLineOrderedSps.Length > 0 && sourceRepository.IsLeftCurlyBraceSequencePoint(sourceLineOrderedSps[0])) {
                            startOffset = sourceLineOrderedSps[0].Offset;
                            // find method "}" offset
                            if (sourceLineOrderedSps.Length > 1 && sourceRepository.IsRightCurlyBraceSequencePoint(sourceLineOrderedSps.Last())) {
                                finalOffset = sourceLineOrderedSps.Last().Offset;
                            }
                        }
                        // find method "{" offset
                        else if (sourceLineOrderedSps.Length > 1 && sourceRepository.IsLeftCurlyBraceSequencePoint(sourceLineOrderedSps[1])) {
                            startOffset = sourceLineOrderedSps[1].Offset;
                            // find method "}" offset
                            if (sourceLineOrderedSps.Length > 2 && sourceRepository.IsRightCurlyBraceSequencePoint(sourceLineOrderedSps.Last())) {
                                finalOffset = sourceLineOrderedSps.Last().Offset;
                            }
                        } else { // "{" not found, try to find "}" offset
                            if (sourceLineOrderedSps.Length > 1 && sourceRepository.IsRightCurlyBraceSequencePoint(sourceLineOrderedSps.Last())) {
                                finalOffset = sourceLineOrderedSps.Last().Offset;
                            }
                        }
                    }

                    // Method offsets found or not found, now check foreach sequence point
                    foreach (var sp in method.SequencePoints) {
                        if (sp != null
                            && sp.FileId == method.FileRef.UniqueId
                            && sp.BranchPoints != null
                            && sp.BranchPoints.Count != 0) {
                        } else {
                            continue;
                        }

                        if (sp.Offset <= startOffset || sp.Offset >= finalOffset) {
                            // doRemoveBranches where .Offset <= startOffset"{" or finalOffset"}" <= .Offset
                            // this will exclude "{" and "}" compiler generated branches and majority of ccrewrite Code-Contract's
                            sp.BranchPoints = new List<BranchPoint>();
                        } else { // branches not removed
                            // check for other options by reading SequencePoint source
                            var text = sourceRepository.GetSequencePointText(sp); // text is never null
                            if (text == string.Empty) {
                                ("Empty sequence-point at line: " + sp.StartLine + " column: " + sp.StartColumn).InformUser();
                                ("Source file: " + source.FilePath).InformUser();
                                return;
                            }
                            // Contract.Requires/Ensures is occasionally left inside method offset
                            // Quick check for minimum length and "C" before using Regex
                            if (text.Length > 18 && text[0] == 'C') {
                                // Use Regex here! "Contract" and "." and "Requires/Ensures"
                                // can be separated by spaces and newlines
                                if (ContractRegex.IsMatch(text)) {
                                    sp.BranchPoints = new List<BranchPoint>();
                                }
                            // "in" keyword?
                            } else if (text == "in") {
                                // Remove generated ::MoveNext branches within "in" keyword
                                // Not always removed in CecilSymbolManager (enumerated KeyValuePair)
                                sp.BranchPoints = new List<BranchPoint>();
                            }
                        }
                    }

                    #endregion

                }
                else {
                    // Do as much possible without source
                    // This will remove generated branches within "{", "}" and "in" (single-line SequencePoints)
                    // but cannot remove Code Contract ccrewite generated branches
                    foreach (var sp in method.SequencePoints) {
                        if (sp != null
                            && sp.BranchPoints != null
                            && sp.BranchPoints.Count != 0
                            && sp.StartLine == sp.EndLine
                            && sp.EndColumn - sp.StartColumn <= 2
                           ) {
                            // Zero, one or two character sequence point should not contain branches
                            // Never found 0 character sequencePoint
                            // Never found 1 character sequencePoint except "{" and "}"
                            // Never found 2 character sequencePoint except "in" keyword
                            // Afaik, c# cannot express branch condition in one or two characters of source code
                            // x|y if(x) while(x) switch(x){...} case: x?. x?? x==y x?y:z; for(...) foreach(...)  x is y
                            // "do" keyword does not generate SequencePoint 
                            sp.BranchPoints = new List<BranchPoint>();
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

                #region Merge Branch-Exits for each Sequence

                // Collection of validBranchPoints (child/connected to parent SequencePoint)
                var validBranchPoints = new List<BranchPoint>();
                var branchExits = new Dictionary<int, BranchPoint>();
                foreach (var sp in method.SequencePoints) {
                    // SequencePoint has branches attached?
                    if (sp.BranchPoints != null && sp.BranchPoints.Count != 0) {
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

                #endregion
            }
        }

        
        private static void TransformSequences_RemoveFalsePositiveUnvisited (IList<Method> methods, SourceRepository sourceRepository, DateTime moduleTime)
        {
            // From Methods with Source and visited SequencePoints
            var sequencePointsQuery = methods
                .Where(m => m != null
                       && m.FileRef != null
                       && m.FileRef.UniqueId != 0
                       && m.SequencePoints != null
                       && m.SequencePoints.Length != 0
                )
                .SelectMany (m => m.SequencePoints)
                .Where(sp => sp != null && sp.FileId != 0 && sp.VisitCount != 0);

            var sequencePointsSet = new HashSet<SequencePoint>(new SequencePointComparer());
            var toRemoveMethodSequencePoint = new List<Tuple<Method, SequencePoint>>();

            // Add unique visited SequencePoints to HashSet
            foreach (var sp in sequencePointsQuery) {
                if (!sequencePointsSet.Contains(sp)) {
                    sequencePointsSet.Add(sp);
                }
            }

            // select false-positive-unvisited
            foreach (var method in methods) {
                if (method != null
                    && method.FileRef != null
                    && method.FileRef.UniqueId != 0
                    && method.SequencePoints != null
                    && method.SequencePoints.Length != 0
                    && method.IsGenerated) {
                } else {
                    continue;
                }

                // Select duplicate and false-positive unvisited sequence points
                // (Sequence point duplicated by generated method and left unvisited)
                foreach (var sp in method.SequencePoints) {
                    if (sp != null
                        && sp.FileId == method.FileRef.UniqueId
                        && sp.VisitCount == 0) { // unvisited only
                    } else {
                        continue;
                    }
                    if (sequencePointsSet.Contains(sp)) {
                        // Unvisited duplicate found, add to remove list
                        toRemoveMethodSequencePoint.Add (new Tuple<Method, SequencePoint>(method, sp));
                    }
                }

                // Select false unvisited right-curly-braces at generated "MoveNext" method
                // (Curly braces moved to generated "MoveNext" method and left unvisited)
                // Source is required here to identify curly braces
                if (method.CallName == "MoveNext") {

                    // Get method source if availabile 
                    var source = sourceRepository.GetCodeCoverageStringTextSource(method.FileRef.UniqueId);

                    // Do we have C# source?
                    if (source != null 
                        && source.FileFound 
                        && source.FileType == FileType.CSharp
                        && source.FileTime <= moduleTime
                       ) {

                        int countDown = 2; // remove up to two last right-curly-braces
                        foreach (var sp in method.SequencePoints.Reverse()) {
                            if (sp != null
                                && sp.FileId != 0
                                && sp.StartLine == sp.EndLine
                                && sp.StartColumn + 1 == sp.EndColumn
                                && sp.VisitCount == 0 // unvisited only
                               ) {
                            } else {
                                continue;
                            }
                            if (countDown > 0) {
                                if (sourceRepository.IsRightCurlyBraceSequencePoint(sp)) {
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

            // Remove selected SequencePoints
            foreach (var tuple in toRemoveMethodSequencePoint) {
                tuple.Item1.SequencePoints = tuple.Item1.SequencePoints.Where(sp => sp != tuple.Item2).ToArray();
            }

            #region ToDo?
            /* Problems:
                         * Compiler can move SequencePoint into compiler generated method
                         * Solution?
                         *  Identify compiler generated methods
                         *   Match each with user method
                         *    Move SequencePoints & branches into user method
                         */
            #endregion

            #region Examples
                        /* Duplicate SequencePoint Example
        Generated Method * <Method visited="false" cyclomaticComplexity="1" sequenceCoverage="0" branchCoverage="0" isConstructor="false" isStatic="false" isGetter="false" isSetter="false">
                         *   <Summary numSequencePoints="1" visitedSequencePoints="0" numBranchPoints="1" visitedBranchPoints="0" sequenceCoverage="0" branchCoverage="0" maxCyclomaticComplexity="1" minCyclomaticComplexity="1" visitedClasses="0" numClasses="0" visitedMethods="0" numMethods="1" />
                         *   <MetadataToken>100663434</MetadataToken>
        <_SetItems>b__b  *   <Name>System.Boolean DD.Collections.BitSetArray::&lt;_SetItems&gt;b__b(System.Int32)</Name>
                         *   <FileRef uid="1" />
                         *   <SequencePoints>
        Duplicate SP!    *     <SequencePoint vc="0" uspid="3648" ordinal="0" offset="0" sl="2653" sc="90" el="2653" ec="109" bec="0" bev="0" fileid="1" />
                         *   </SequencePoints>
                         *   <BranchPoints />
                         *   <MethodPoint xsi:type="SequencePoint" vc="0" uspid="3648" ordinal="0" offset="0" sl="2653" sc="90" el="2653" ec="109" bec="0" bev="0" fileid="1" />
                         * </Method>
                         *
        Generated Method * <Method visited="true" cyclomaticComplexity="1" sequenceCoverage="100" branchCoverage="100" isConstructor="false" isStatic="false" isGetter="false" isSetter="false">
                         *   <Summary numSequencePoints="1" visitedSequencePoints="1" numBranchPoints="1" visitedBranchPoints="1" sequenceCoverage="100" branchCoverage="100" maxCyclomaticComplexity="1" minCyclomaticComplexity="1" visitedClasses="0" numClasses="0" visitedMethods="1" numMethods="1" />
                         *   <MetadataToken>100663435</MetadataToken>
        <_SetItems>b__b_0*   <Name>System.Boolean DD.Collections.BitSetArray::BitSetArray_&lt;_SetItems&gt;b__b_0(System.Int32)</Name>
                         *   <FileRef uid="1" />
                         *   <SequencePoints>
        Duplicate SP!    *     <SequencePoint vc="6081" uspid="3649" ordinal="0" offset="0" sl="2653" sc="90" el="2653" ec="109" bec="0" bev="0" fileid="1" />
                         *   </SequencePoints>
                         *   <BranchPoints />
                         *   <MethodPoint xsi:type="SequencePoint" vc="6081" uspid="3649" ordinal="0" offset="0" sl="2653" sc="90" el="2653" ec="109" bec="0" bev="0" fileid="1" />
                         * </Method>
                         *
        User Method      * <Method visited="true" cyclomaticComplexity="9" sequenceCoverage="100" branchCoverage="100" isConstructor="false" isStatic="false" isGetter="false" isSetter="false">
                         *   <Summary numSequencePoints="41" visitedSequencePoints="41" numBranchPoints="9" visitedBranchPoints="9" sequenceCoverage="100" branchCoverage="100" maxCyclomaticComplexity="9" minCyclomaticComplexity="9" visitedClasses="0" numClasses="0" visitedMethods="1" numMethods="1" />
                         *   <MetadataToken>100663375</MetadataToken>
                         *   <Name>System.Void DD.Collections.BitSetArray::_SetItems(System.Collections.Generic.IEnumerable`1&lt;System.Int32&gt;)</Name>
                         *   <FileRef uid="1" />
                         *   <SequencePoints>
                         *     <SequencePoint vc="760" uspid="2609" ordinal="0" offset="0" sl="2649" sc="13" el="2649" ec="77" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="760" uspid="2610" ordinal="1" offset="47" sl="2650" sc="13" el="2650" ec="76" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="759" uspid="2611" ordinal="2" offset="68" sl="2651" sc="13" el="2651" ec="75" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="758" uspid="2612" ordinal="3" offset="87" sl="2652" sc="13" el="2652" ec="70" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="757" uspid="2613" ordinal="4" offset="105" sl="2653" sc="13" el="2653" ec="112" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2614" ordinal="5" offset="168" sl="2648" sc="42" el="2648" ec="43" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2615" ordinal="6" offset="171" sl="2659" sc="13" el="2659" ec="28" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2616" ordinal="7" offset="188" sl="2659" sc="29" el="2659" ec="30" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2617" ordinal="8" offset="189" sl="2660" sc="17" el="2660" ec="36" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2618" ordinal="9" offset="196" sl="2661" sc="17" el="2661" ec="45" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2619" ordinal="10" offset="202" sl="2662" sc="17" el="2662" ec="45" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2620" ordinal="11" offset="208" sl="2663" sc="17" el="2663" ec="24" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2621" ordinal="12" offset="209" sl="2663" sc="38" el="2663" ec="43" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6076" uspid="2622" ordinal="13" offset="222" sl="2663" sc="26" el="2663" ec="34" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6076" uspid="2623" ordinal="14" offset="230" sl="2663" sc="45" el="2663" ec="46" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6076" uspid="2624" ordinal="15" offset="231" sl="2664" sc="21" el="2664" ec="82" bec="2" bev="2" fileid="1" />
                         *     <SequencePoint vc="8" uspid="2625" ordinal="16" offset="265" sl="2664" sc="83" el="2664" ec="84" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="8" uspid="2626" ordinal="17" offset="266" sl="2666" sc="21" el="2666" ec="22" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2627" ordinal="18" offset="272" sl="2667" sc="26" el="2667" ec="27" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2628" ordinal="19" offset="273" sl="2669" sc="25" el="2669" ec="76" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2629" ordinal="20" offset="301" sl="2670" sc="25" el="2670" ec="67" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2630" ordinal="21" offset="327" sl="2671" sc="25" el="2671" ec="41" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2631" ordinal="22" offset="341" sl="2672" sc="25" el="2672" ec="45" bec="2" bev="2" fileid="1" />
                         *     <SequencePoint vc="770" uspid="2632" ordinal="23" offset="357" sl="2672" sc="46" el="2672" ec="47" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="770" uspid="2633" ordinal="24" offset="358" sl="2673" sc="29" el="2673" ec="45" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="770" uspid="2634" ordinal="25" offset="360" sl="2674" sc="25" el="2674" ec="26" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2635" ordinal="26" offset="361" sl="2675" sc="25" el="2675" ec="45" bec="2" bev="2" fileid="1" />
                         *     <SequencePoint vc="6050" uspid="2636" ordinal="27" offset="377" sl="2675" sc="46" el="2675" ec="47" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6050" uspid="2637" ordinal="28" offset="378" sl="2676" sc="29" el="2676" ec="45" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6050" uspid="2638" ordinal="29" offset="380" sl="2677" sc="25" el="2677" ec="26" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6068" uspid="2639" ordinal="30" offset="381" sl="2678" sc="21" el="2678" ec="22" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6076" uspid="2640" ordinal="31" offset="382" sl="2679" sc="17" el="2679" ec="18" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="6828" uspid="2641" ordinal="32" offset="383" sl="2663" sc="35" el="2663" ec="37" bec="2" bev="2" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2642" ordinal="33" offset="428" sl="2680" sc="17" el="2680" ec="47" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2643" ordinal="34" offset="436" sl="2681" sc="17" el="2681" ec="46" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2644" ordinal="35" offset="444" sl="2682" sc="17" el="2682" ec="50" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2645" ordinal="36" offset="465" sl="2683" sc="13" el="2683" ec="14" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2646" ordinal="37" offset="494" sl="2684" sc="9" el="2684" ec="10" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2647" ordinal="38" offset="522" sl="2655" sc="13" el="2655" ec="47" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2648" ordinal="39" offset="542" sl="2656" sc="13" el="2656" ec="52" bec="0" bev="0" fileid="1" />
                         *     <SequencePoint vc="752" uspid="2649" ordinal="40" offset="568" sl="2657" sc="13" el="2657" ec="51" bec="0" bev="0" fileid="1" />
                         *   </SequencePoints>
                         *   <BranchPoints>
                         *     <BranchPoint vc="8" uspid="2652" ordinal="2" offset="260" sl="2664" path="0" offsetend="265" fileid="1" />
                         *     <BranchPoint vc="6068" uspid="2653" ordinal="3" offset="260" sl="2664" path="1" offsetend="272" fileid="1" />
                         *     <BranchPoint vc="770" uspid="2654" ordinal="4" offset="352" sl="2672" path="0" offsetend="357" fileid="1" />
                         *     <BranchPoint vc="5298" uspid="2655" ordinal="5" offset="352" sl="2672" path="1" offsetend="361" fileid="1" />
                         *     <BranchPoint vc="6050" uspid="2656" ordinal="6" offset="372" sl="2675" path="0" offsetend="377" fileid="1" />
                         *     <BranchPoint vc="18" uspid="2657" ordinal="7" offset="372" sl="2675" path="1" offsetend="381" fileid="1" />
                         *     <BranchPoint vc="752" uspid="2658" ordinal="8" offset="394" sl="2663" path="0" offsetend="399" fileid="1" />
                         *     <BranchPoint vc="6076" uspid="2659" ordinal="9" offset="394" sl="2663" path="1" offsetend="222" fileid="1" />
                         *   </BranchPoints>
                         *   <MethodPoint xsi:type="SequencePoint" vc="760" uspid="2609" ordinal="0" offset="0" sl="2649" sc="13" el="2649" ec="77" bec="0" bev="0" fileid="1" />
                         * </Method>
                         *
                         */
            #endregion
        }
        
    }
}
