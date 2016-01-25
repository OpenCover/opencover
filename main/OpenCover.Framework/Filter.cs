//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.IO;
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
        public Filter(bool useRegexFilters = false)
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
        /// <param name="processPath">The path-name of the process being profiled</param>
        /// <param name="assemblyPath">The path-name of the assembly under profile</param>
        /// <remarks>All assemblies matching either the inclusion or exclusion filter should be included 
        /// as it is the class that is being filtered within these unless the class filter is *</remarks>
        public bool UseAssembly(string processPath, string assemblyPath)
        {
            string assemblyName;
            bool assemblyNameAdd;
            IList<AssemblyAndClassFilter> matchingExclusionFilters;
            if (!ExtractExclusionFilters(processPath, assemblyPath, out assemblyName, out assemblyNameAdd, out matchingExclusionFilters)) 
                return false;

            if (matchingExclusionFilters.Any(exclusionFilter => exclusionFilter.ClassName != ".*"))
                return true;

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyPath);
            if (assemblyNameAdd) { matchingInclusionFilters.AddRange (InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName)); }
            return matchingInclusionFilters.Any();
        }

        private bool ExtractExclusionFilters(string processPath, string assemblyPath, out string assemblyName,
            out bool assemblyNameAdd, out IList<AssemblyAndClassFilter> matchingExclusionFilters)
        {
            var processName = string.Empty;
            var processNameAdd = false;
            if (processPath.IndexOfAny(Path.GetInvalidPathChars()) < 0)
            {
                // avoids ArgumentException
                processName = Path.GetFileNameWithoutExtension(processPath);
                processNameAdd = !string.IsNullOrEmpty(processName) && processName != processPath;
            }
            assemblyName = string.Empty;
            assemblyNameAdd = false;
            if (assemblyPath.IndexOfAny(Path.GetInvalidPathChars()) < 0)
            {
                // avoids ArgumentException
                assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                assemblyNameAdd = !string.IsNullOrEmpty(assemblyName) && assemblyName != assemblyPath;
            }
            matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForAssemblyName(assemblyPath);
            if (assemblyNameAdd)
            {
                matchingExclusionFilters.AddRange(ExclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName));
            }
            return !matchingExclusionFilters.Any(
                exclusionFilter =>
                    exclusionFilter.ClassName == ".*" &&
                    ((processNameAdd && exclusionFilter.IsMatchingProcessName(processName)) ||
                     exclusionFilter.IsMatchingProcessName(processPath)));
        }

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="processPath">The path-name of the process</param>
        /// <param name="assemblyPath">the name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        public bool InstrumentClass(string processPath, string assemblyPath, string className)
        {
            if (string.IsNullOrEmpty(processPath) || string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(className))
            {
                return false;
            }

            string assemblyName;
            bool assemblyNameAdd;
            IList<AssemblyAndClassFilter> matchingExclusionFilters;
            if (!ExtractExclusionFilters(processPath, assemblyPath, out assemblyName, out assemblyNameAdd, out matchingExclusionFilters)) 
                return false;

            if (matchingExclusionFilters
                .Where(exclusionFilter => exclusionFilter.ClassName != ".*")
                .Any(exclusionFilter => exclusionFilter.IsMatchingClassName(className)))
            {
                return false;
            }

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyPath);
            if (assemblyNameAdd) { matchingInclusionFilters.AddRange (InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName)); }
            return matchingInclusionFilters.Any(inclusionFilter => inclusionFilter.IsMatchingClassName(className));
        }


        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="assemblyPath">The path-name of the assembly under profile</param>
        /// <param name="className">The name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        public bool InstrumentClass(string assemblyPath, string className)
        {
            return InstrumentClass(Guid.NewGuid().ToString(), assemblyPath, className);
        }

        /// <summary>
        /// Add a filter
        /// </summary>
        /// <param name="processAssemblyClassFilter">Filter is of the format (+ or -)&lt;processFilter&gt;[assemblyFilter]classFilter, wildcards are allowed. <br/>
        /// i.e. -[mscorlib], -[System.*]*, +[App.*]*, +[*]*
        /// </param>
        public void AddFilter(string processAssemblyClassFilter)
        {
            string assemblyFilter;
            string classFilter;
            string processFilter;
            FilterType filterType;
            GetAssemblyClassName(processAssemblyClassFilter, RegExFilters, out filterType, out assemblyFilter, out classFilter, out processFilter);

            try
            {
                if (!RegExFilters)
                {
                    processFilter = (string.IsNullOrEmpty(processFilter) ? "*" : processFilter).ValidateAndEscape("<>|\""); // Path.GetInvalidPathChars except *?
                    assemblyFilter = assemblyFilter.ValidateAndEscape();
                    classFilter = classFilter.ValidateAndEscape();
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

        private static void HandleInvalidFilterFormat(string assemblyClassName)
        {
            Logger.ErrorFormat("Unable to process the filter '{0}'. Please check your syntax against the usage guide and try again.", assemblyClassName);
            Logger.ErrorFormat("The usage guide can also be found at https://github.com/OpenCover/opencover/wiki/Usage.");
            throw new ExitApplicationWithoutReportingException();
        }

        /// <summary>
        /// Add attribute exclusion filters
        /// </summary>
        /// <param name="exclusionFilters">An array of filters that are used to wildcard match an attribute</param>
        public void AddAttributeExclusionFilters(string[] exclusionFilters)
        {
            ExcludedAttributes.AddFilters(exclusionFilters, RegExFilters);
        }

        /// <summary>
        /// Is this entity (method/type) excluded due to an attributeFilter
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <returns></returns>
        public bool ExcludeByAttribute(IMemberDefinition entity)
        {
            if (ExcludedAttributes.Count == 0)
                return false;

            while (true)
            {
                if (entity == null)
                    return false;

                if (ExcludeByAttribute((ICustomAttributeProvider)entity))
                    return true;

                if (ExcludeByAttribute(entity.DeclaringType))
                    return true;

                if (entity.DeclaringType == null || !entity.Name.StartsWith("<"))
                    return false;

                var match = Regex.Match(entity.Name, @"\<(?<name>.+)\>.+");
                if (match.Groups["name"] == null) return false;
                var name = match.Groups["name"].Value;
                var target = entity.DeclaringType.Methods.FirstOrDefault(m => m.Name == name);
                if (target == null) return false;
                if (target.IsGetter)
                {
                    var getMethod = entity.DeclaringType.Properties.FirstOrDefault(p => p.GetMethod == target);
                    entity = getMethod;
                    continue;
                }
                if (target.IsSetter)
                {
                    var setMethod = entity.DeclaringType.Properties.FirstOrDefault(p => p.SetMethod == target);
                    entity = setMethod;
                    continue;
                }
                entity = target;
            }
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
            ExcludedFiles.AddFilters(exclusionFilters, RegExFilters);
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
            TestFiles.AddFilters(testFilters, RegExFilters);
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
        /// Should we instrument this asssembly
        /// </summary>
        /// <param name="processPath"></param>
        /// <returns></returns>
        public bool InstrumentProcess(string processPath)
        {
            if (string.IsNullOrEmpty(processPath))
            {
                return false;
            }
            if (!ExclusionFilters.Any() && !InclusionFilters.Any()) return true;

            var processName = string.Empty;
            var processNameAdd = false;
            if (processPath.IndexOfAny(Path.GetInvalidPathChars()) < 0) { // avoids ArgumentException
                processName = Path.GetFileNameWithoutExtension(processPath);
                processNameAdd = !string.IsNullOrWhiteSpace(processName) && processName != processPath;
            }
            if (ExclusionFilters.Any()) {
                var matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForProcessName(processPath);
                if (processNameAdd) {
                	matchingExclusionFilters.AddRange(ExclusionFilters.GetMatchingFiltersForProcessName(processName));
                }
                if (matchingExclusionFilters.Any
                        ( exclusionFilter =>
                            // Excluded by all filters
                            (exclusionFilter.AssemblyName == ".*" && exclusionFilter.ClassName == ".*")
                        )
                    )
                {
                    return false;
                }
            }

            if (InclusionFilters.Any()) {
                var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForProcessName(processPath);
                if (processNameAdd) {
                    matchingInclusionFilters.AddRange(InclusionFilters.GetMatchingFiltersForProcessName(processName));
                }
                return matchingInclusionFilters.Any();
            }

            return true; // not excluded and no inclusion filters
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

            return filter;
        }
    }
}
