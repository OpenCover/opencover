//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using log4net;
using Mono.Cecil;
using OpenCover.Framework.Filtering;

namespace OpenCover.Framework
{
    /// <summary>
    ///  A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public class Filter : IFilter
    {
        private static readonly ILog Logger = LogManager.GetLogger("OpenCover");

        internal IList<AssemblyAndClassFilter> InclusionFilters { get; private set; }
        internal IList<AssemblyAndClassFilter> ExclusionFilters { get; private set; }
        internal IList<RegexFilter> ExcludedAttributes { get; private set; }
        internal IList<RegexFilter> ExcludedFiles { get; private set; }
        internal IList<RegexFilter> TestFiles { get; private set; }

        /// <summary>
        /// Are the filters supplied as reguar expressions
        /// </summary>
        public bool RegExFilters { get; private set; }
        

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="useRegexFilters">Indicates if the input strings for this class are already Regular Expressions</param>
        public Filter(bool useRegexFilters)
        {
            InclusionFilters = new List<AssemblyAndClassFilter>();
            ExclusionFilters = new List<AssemblyAndClassFilter>();
            ExcludedAttributes = new List<RegexFilter>();
            ExcludedFiles = new List<RegexFilter>();
            TestFiles = new List<RegexFilter>();
            RegExFilters = useRegexFilters;
        }

        /// <summary>
        /// Decides whether an assembly should be included in the instrumentation
        /// </summary>
        /// <param name="processName">The name of the process being profiled</param>
        /// <param name="assemblyName">The name of the assembly under profile</param>
        /// <remarks>All assemblies matching either the inclusion or exclusion filter should be included 
        /// as it is the class that is being filtered within these unless the class filter is *</remarks>
        public bool UseAssembly(string processName, string assemblyName)
        {
            if (ExcludeProcessOrAssembly(processName, assemblyName, out IList<AssemblyAndClassFilter> matchingExclusionFilters))
                return false;

            if (matchingExclusionFilters.Any(exclusionFilter => exclusionFilter.ClassName != ".*"))
                return true;

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);

            return matchingInclusionFilters.Any();
        }

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        public bool InstrumentClass(string processName, string assemblyName, string className)
        {
            if (string.IsNullOrEmpty(processName) || string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(className))
            {
                return false;
            }

            if (ExcludeProcessOrAssembly(processName, assemblyName, out IList<AssemblyAndClassFilter> matchingExclusionFilters))
                return false;

            if (matchingExclusionFilters
                .Where(exclusionFilter => exclusionFilter.ClassName != ".*")
                .Any(exclusionFilter => exclusionFilter.IsMatchingClassName(className)))
            {
                return false;
            }

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);

            return matchingInclusionFilters.Any(inclusionFilter => inclusionFilter.IsMatchingClassName(className));
        }

        private bool ExcludeProcessOrAssembly(string processName, string assemblyName, out IList<AssemblyAndClassFilter> matchingExclusionFilters)
        {
            matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);
            return matchingExclusionFilters.Any(
                exclusionFilter =>
                    exclusionFilter.ClassName == ".*" &&
                    (exclusionFilter.IsMatchingProcessName(processName)));
        }

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="assemblyName">The name of the assembly under profile</param>
        /// <param name="className">The name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        public bool InstrumentClass(string assemblyName, string className)
        {
            return InstrumentClass(Guid.NewGuid().ToString(), assemblyName, className);
        }

        /// <summary>
        /// Add a filter
        /// </summary>
        /// <param name="processAssemblyClassFilter">Filter is of the format (+ or -)&lt;processFilter&gt;[assemblyFilter]classFilter, wildcards are allowed. <br/>
        /// i.e. -[mscorlib], -[System.*]*, +[App.*]*, +[*]*
        /// </param>
        public void AddFilter(string processAssemblyClassFilter)
        {
            GetAssemblyClassName(processAssemblyClassFilter, RegExFilters, out FilterType filterType,
                out string assemblyFilter, out string classFilter, out string processFilter);

            try
            {
                if (!RegExFilters)
                {
                    processFilter = ValidateAndEscape((string.IsNullOrEmpty(processFilter) ? "*" : processFilter), "<>|\"", "process"); // Path.GetInvalidPathChars except *?
                    assemblyFilter = ValidateAndEscape(assemblyFilter, @"\[]", "assembly");
                    classFilter = ValidateAndEscape(classFilter, @"\[]", "class/type");
                }

                var filter = new AssemblyAndClassFilter(processFilter, assemblyFilter, classFilter);
                if (filterType == FilterType.Inclusion)
                    InclusionFilters.Add(filter);

                if (filterType == FilterType.Exclusion)
                    ExclusionFilters.Add(filter);
            }
            catch (Exception)
            {
                HandleInvalidFilterFormat(processAssemblyClassFilter);
            }
        }

        private static void GetAssemblyClassName(string processAssemblyClassFilter, bool useRegEx, out FilterType filterType, out string assemblyFilter, out string classFilter, out string processFilter)
        {
            classFilter = string.Empty;
            assemblyFilter = string.Empty;
            processFilter = string.Empty;
            filterType = FilterType.Inclusion;
            var regEx = new Regex(@"^(?<type>([+-]))(<(?<process>(.+))>)?(\[(?<assembly>(.+))\])(?<class>(.+))$");
            if (useRegEx)
                regEx = new Regex(@"^(?<type>([+-]))(<\((?<process>(.+))\)>)?(\[\((?<assembly>(.+))\)\])(\((?<class>(.+))\))$");

            var match = regEx.Match(processAssemblyClassFilter);
            if (match.Success)
            {
                filterType = match.Groups["type"].Value.ParseFilterType();
                assemblyFilter = match.Groups["assembly"].Value;
                classFilter = match.Groups["class"].Value;
                processFilter = match.Groups["process"].Value;

                if (string.IsNullOrWhiteSpace(assemblyFilter))
                    HandleInvalidFilterFormat(processAssemblyClassFilter);
            }
            else
            {
                HandleInvalidFilterFormat(processAssemblyClassFilter);
            }
        }

        private static void HandleInvalidFilterFormat(string filter)
        {
            Logger.ErrorFormat("Unable to process the filter '{0}'. Please check your syntax against the usage guide and try again.", filter);
            Logger.ErrorFormat("The usage guide can also be found at https://github.com/OpenCover/opencover/wiki/Usage.");
            throw new ExitApplicationWithoutReportingException();
        }

        /// <summary>
        /// Add attribute exclusion filters
        /// </summary>
        /// <param name="exclusionFilters">An array of filters that are used to wildcard match an attribute</param>
        public void AddAttributeExclusionFilters(string[] exclusionFilters)
        {
            AddFilters(ExcludedAttributes, exclusionFilters, RegExFilters, "attribute");
        }

        /// <summary>
        /// Is this entity (method/type) excluded due to an attributeFilter
        /// </summary>
        /// <param name="originalEntity">The entity to test</param>
        /// <returns></returns>
        public bool ExcludeByAttribute(IMemberDefinition originalEntity)
        {
            if (ExcludedAttributes.Count == 0)
                return false;

            var entity = originalEntity;
            while (entity != null)
            {
                if (IsExcludedByAttributeSimple(entity, out bool excludeByAttribute))
                    return excludeByAttribute;

                entity = GetDeclaringMethod(entity);
            }
            return false;
        }

        /// <summary>
        /// Look for the declaring method e.g. if method is some type of lambda, getter/setter etc
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static IMemberDefinition GetDeclaringMethod(IMemberDefinition entity)
        {
            if (!MatchDeclaringMethod(entity, out MethodDefinition target))
                return null;

            if (target.IsGetter || target.IsSetter)
            {
                return entity.DeclaringType.Properties.FirstOrDefault(p => p.GetMethod == target || p.SetMethod == target);
            }
            return target;
        }

        private static bool MatchDeclaringMethod(IMemberDefinition entity, out MethodDefinition target)
        {
            target = null;
            var match = Regex.Match(entity.Name, @"\<(?<name>.+)\>.+");
            if (match.Groups["name"] == null)
                return false;

            var name = match.Groups["name"].Value;
            target = entity.DeclaringType.Methods.FirstOrDefault(m => m.Name == name);
            if (target == null)
                return false;
            return true;
        }

        private bool IsExcludedByAttributeSimple(IMemberDefinition entity, out bool excludeByAttribute)
        {
            excludeByAttribute = true;
            if (ExcludeByAttribute((ICustomAttributeProvider) entity))
                return true;

            if (ExcludeByAttribute(entity.DeclaringType))
                return true;

            if (entity.DeclaringType != null && entity.Name.StartsWith("<")) 
                return false;
            
            excludeByAttribute = false;
            return true;
        }

        private bool ExcludeByAttribute(ICustomAttributeProvider entity)
        {
            return (from excludeAttribute in ExcludedAttributes
                from customAttribute in entity.CustomAttributes
                where excludeAttribute.IsMatchingExpression(customAttribute.AttributeType.FullName)
                select excludeAttribute).Any();
        }

        /// <summary>
        /// Is this entity excluded due to an attributeFilter
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <returns></returns>
        public bool ExcludeByAttribute(AssemblyDefinition entity)
        {
            return ExcludedAttributes.Count != 0 && ExcludeByAttribute((ICustomAttributeProvider)entity);
        }

        /// <summary>
        /// Is this file excluded
        /// </summary>
        /// <param name="fileName">The name of the file to test</param>
        /// <returns></returns>
        public bool ExcludeByFile(string fileName)
        {
            if (ExcludedFiles.Count == 0 || string.IsNullOrWhiteSpace(fileName))
                return false;

            return ExcludedFiles.Any(excludeFile => excludeFile.IsMatchingExpression(fileName));
        }

        /// <summary>
        /// Add file exclusion filters
        /// </summary>
        /// <param name="exclusionFilters"></param>
        public void AddFileExclusionFilters(string[] exclusionFilters)
        {
            AddFilters(ExcludedFiles, exclusionFilters, RegExFilters, "file exclusion");
        }

        /// <summary>
        /// Decides whether an assembly should be analysed for test methods
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <returns>true - if the assembly matches the test assembly filter</returns>
        public bool UseTestAssembly(string assemblyName)
        {
            if (TestFiles.Count == 0 || string.IsNullOrWhiteSpace(assemblyName))
                return false;

            return TestFiles.Any(file => file.IsMatchingExpression(assemblyName));
        }

        /// <summary>
        /// Add test file filters
        /// </summary>
        /// <param name="testFilters"></param>
        public void AddTestFileFilters(string[] testFilters)
        {
            AddFilters(TestFiles, testFilters, RegExFilters, "test assembly");
        }

        /// <summary>
        /// Is the method an auto-implemented property get/set
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool IsAutoImplementedProperty(MethodDefinition method)
        {
            if ((method.IsSetter || method.IsGetter) && method.HasCustomAttributes)
            {
                return method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
            }
            return false;
        }

        /// <summary>
        /// Is the method an F# implementation detail
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <remarks>This may need to be extended for other F# versions than 4.1</remarks>
        public bool IsFSharpInternal(MethodDefinition method)
        {
            // Discriminated Union/Sum/Algebraic data types are implemented as
            // subtypes nested in the base type
            var baseType = Enumerable.Empty<CustomAttribute>();
            if (method.DeclaringType.DeclaringType != null
                && method.DeclaringType.DeclaringType.HasCustomAttributes)
            {
                baseType = method.DeclaringType.DeclaringType.CustomAttributes;
            }

            // Algebraic types have debug proxies nested in the base type which are not attributed at the type level
            if (!method.HasCustomAttributes && !(method.DeclaringType.HasCustomAttributes || baseType.Any()))
            {
                return false;
            }

            // Use string literals rather than adding F# as a dependency
            // as it's not a part of the default VS2017 desktop install,
            // and to avoid having to install it ourselves with the
            // opencover binaries
            var mappings = method.DeclaringType.CustomAttributes.Concat(baseType)
                                 .Where(x => x.AttributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
                                 .ToList();

            if (!mappings.Any())
            {
                return false;
            }

            // Getting attribute constructor arguments for the F# types needs 
            // the F# core assembly to resolve them; look to the raw binary instead
            mappings = mappings
                .Where(x =>
                {
                    // expect 01, 00, 01, 00, 00, 00, 00, 00 or 01, 00, 02, 00, 00, 00, 00, 00
                    var y = x.GetBlob();
                    if (y.Length != 8 || y[0] != 1 || y[1] != 0 || y.Skip(3).Any(z => z != 0))
                    {
                        return false;
                    }

                    // Mask out the class kind from its public/non-public state
                    // SourceConstructFlags.NonPublicRepresentation = 32
                    // SourceConstructFlags.KindMask = 31
                    return (y[2] & 31).Equals(1)  // SourceConstructFlags.SumType = 1
                        || (y[2] & 31).Equals(2); // SourceConstructFlags.RecordType = 2
                }).ToList();

            var fieldGetter = false;
            if (method.IsGetter)
            {
                // record type has getters marked as field
                var owner = method.DeclaringType.Properties
                    .Where(x => x.GetMethod == method)
                    .First();
                if (owner.HasCustomAttributes)
                {
                    fieldGetter = owner.CustomAttributes.Where(x => x.AttributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
                        .Any(x => (x.GetBlob()[2] & 31) == 4); // SourceConstructFlags.Field = 4
                }
            }

            // The exclusions list may be overkill
            return mappings.Any() &&
                (method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)
                || method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(System.Diagnostics.DebuggerNonUserCodeAttribute).FullName)
                || method.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
                || method.IsConstructor
                || fieldGetter);
        }

        /// <summary>
        /// Should we instrument this asssembly
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public bool InstrumentProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return false;

            if (!ExclusionFilters.Any() && !InclusionFilters.Any()) 
                return true;

            if (IsProcessExcluded(processName)) 
                return false;

            if (InclusionFilters.Any())
            {
                var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForProcessName(processName);
                return matchingInclusionFilters.Any();
            }

            return true; // not excluded and no inclusion filters
        }

        private bool IsProcessExcluded(string processName)
        {
            if (!ExclusionFilters.Any()) 
                return false;

            var matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForProcessName(processName);
            // Excluded by all filters
            return matchingExclusionFilters.Any(exclusionFilter =>(exclusionFilter.AssemblyName == ".*" && exclusionFilter.ClassName == ".*"));
        }

        readonly IList<string> _excludePaths = new List<string>();

        /// <summary>
        /// Add a folder to the list that modules in these folders (and their children) should be excluded
        /// </summary>
        /// <param name="excludedPath"></param>
        public void AddExcludedFolder(string excludedPath)
        {
            _excludePaths.Add(excludedPath.ToLowerInvariant());
        }

        /// <summary>
        /// Should we use this module based on it's path
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        public bool UseModule(string modulePath)
        {
            return _excludePaths.All(path => !modulePath.ToLowerInvariant().StartsWith(path));
        }

        /// <summary>
        /// Create a filter entity from parser parameters
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IFilter BuildFilter(CommandLineParser parser)
        {
            var filter = new Filter(parser.RegExFilters);

            // apply filters
            if (!parser.NoDefaultFilters)
            {
                if (parser.RegExFilters)
                {
                    filter.AddFilter(@"-[(mscorlib)](.*)");
                    filter.AddFilter(@"-[(mscorlib\..*)](.*)");
                    filter.AddFilter(@"-[(System)](.*)");
                    filter.AddFilter(@"-[(System\..*)](.*)");
                    filter.AddFilter(@"-[(Microsoft.VisualBasic)](.*)");
                }
                else
                {
                    filter.AddFilter("-[mscorlib]*");
                    filter.AddFilter("-[mscorlib.*]*");
                    filter.AddFilter("-[System]*");
                    filter.AddFilter("-[System.*]*");
                    filter.AddFilter("-[Microsoft.VisualBasic]*");
                }
            }

            if (parser.Filters.Count > 0)
            {
                parser.Filters.ForEach(filter.AddFilter);
            }

            filter.AddAttributeExclusionFilters(parser.AttributeExclusionFilters.ToArray());
            filter.AddFileExclusionFilters(parser.FileExclusionFilters.ToArray());
            filter.AddTestFileFilters(parser.TestFilters.ToArray());
            foreach (var excludeDir in parser.ExcludeDirs)
            {
                filter.AddExcludedFolder(excludeDir);
            }
            

            return filter;
        }

        static void AddFilters(ICollection<RegexFilter> target, IEnumerable<string> filters, bool isRegexFilter, string filterType)
        {
            if (filters == null)
                return;

            foreach (var regexFilter in filters.Where(x => x != null).Select(filter => isRegexFilter ? new RegexFilter(filter, false) : new RegexFilter(ValidateAndEscape(filter, @"[]", filterType))))
            {
                target.Add(regexFilter);
            }
        }

        static string ValidateAndEscape(string match, string notAllowed, string filterType)
        {
            if (match.IndexOfAny(notAllowed.ToCharArray()) >= 0)
            {
                Logger.ErrorFormat("The string '{0}' is invalid for a{2} '{1}' filter name", 
                    match, filterType, "aeiou".Contains(filterType[0]) ? "n" : "");
                HandleInvalidFilterFormat(match);
            }
            return match.Replace(@"\", @"\\").Replace(@".", @"\.").Replace(@"*", @".*");
        }
    }
}
