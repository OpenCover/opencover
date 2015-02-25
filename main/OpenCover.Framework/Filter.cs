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
using Mono.Cecil;
using OpenCover.Framework.Filtering;

namespace OpenCover.Framework
{
    internal class AssemblyAndClassFilter
    {
        private readonly RegexFilter assemblyNameFilter;

        private readonly RegexFilter classNameFilter;
        
        internal string AssemblyName { get { return assemblyNameFilter.FilterExpression; } }

        internal string ClassName { get { return classNameFilter.FilterExpression; } }

        internal AssemblyAndClassFilter(string assemblyName, string className)
        {
            assemblyNameFilter = new RegexFilter(assemblyName);
            classNameFilter = new RegexFilter(className);
        }

        internal bool IsMatchingAssemblyName(string assemblyName)
        {
            return assemblyNameFilter.IsMatchingExpression(assemblyName);
        }

        internal bool IsMatchingClassName(string className)
        {
            return classNameFilter.IsMatchingExpression(className);
        }
    }

    /// <summary>
    ///  A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public class Filter : IFilter
    {
        internal IList<AssemblyAndClassFilter> InclusionFilters { get; private set; }
        internal IList<AssemblyAndClassFilter> ExclusionFilters { get; private set; }
        internal IList<Lazy<Regex>> ExcludedAttributes { get; private set; }
        internal IList<Lazy<Regex>> ExcludedFiles { get; private set; }
        internal IList<Lazy<Regex>> TestFiles { get; private set; }
        public bool RegExFilters { get; private set; }

        /// <summary>
        /// Standard constructor
        /// </summary>
        public Filter(bool useRegexFilters = false)
        {
            InclusionFilters = new List<AssemblyAndClassFilter>();
            ExclusionFilters = new List<AssemblyAndClassFilter>();
            ExcludedAttributes = new List<Lazy<Regex>>();
            ExcludedFiles = new List<Lazy<Regex>>();
            TestFiles = new List<Lazy<Regex>>();
            RegExFilters = useRegexFilters;
        }

        public bool UseAssembly(string assemblyName)
        {
            var matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);
            if (matchingExclusionFilters.Any(exclusionFilter => exclusionFilter.ClassName == ".*"))
            {
                return false;
            }

            if (matchingExclusionFilters.Any(exclusionFilter => exclusionFilter.ClassName != ".*"))
            {
                return true;
            }

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);
            if (matchingInclusionFilters.Any())
            {
                return true;
            }

            return false;
        }

        public bool InstrumentClass(string assemblyName, string className)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(className))
            {
                return false;
            }

            var matchingExclusionFilters = ExclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);
            if (matchingExclusionFilters.Any(exclusionFilter => exclusionFilter.ClassName == ".*"))
            {
                return false;
            }

            if (matchingExclusionFilters
                .Where(exclusionFilter => exclusionFilter.ClassName != ".*")
                .Any(exclusionFilter => exclusionFilter.IsMatchingClassName(className)))
            {
                return false;
            }

            var matchingInclusionFilters = InclusionFilters.GetMatchingFiltersForAssemblyName(assemblyName);
            if (matchingInclusionFilters.Any(inclusionFilter => inclusionFilter.IsMatchingClassName(className)))
            {
                return true;
            }

            return false;
        }

        public void AddFilter(string assemblyClassName)
        {
            string assemblyName;
            string className;
            FilterType filterType;
            GetAssemblyClassName(assemblyClassName, RegExFilters, out filterType, out assemblyName, out className);

            if (!RegExFilters)
            {
                assemblyName = assemblyName.ValidateAndEscape();
                className = className.ValidateAndEscape();
            }

            var filter = new AssemblyAndClassFilter(assemblyName, className);
            if (filterType == FilterType.Inclusion)
                InclusionFilters.Add(filter);

            if (filterType == FilterType.Exclusion)
                ExclusionFilters.Add(filter);
        }

        private static void GetAssemblyClassName(string assemblyClassName, bool useRegEx, out FilterType filterType, out string assemblyName, out string className)
        {
            className = string.Empty;
            assemblyName = string.Empty;
            var regEx = new Regex(@"^(?<type>([+-]))(\[(?<assembly>(.+))\])(?<class>(.*))?$");
            if (useRegEx)
                regEx = new Regex(@"^(?<type>([+-]))(\[\((?<assembly>(.+))\)\])(\((?<class>(.*))\))?$");

            var match = regEx.Match(assemblyClassName);
            if (match.Success)
            {
                filterType = match.Groups["type"].Value.ParseFilterType();
                assemblyName = match.Groups["assembly"].Value;
                className = match.Groups["class"].Value;

                if (string.IsNullOrWhiteSpace(assemblyName))
                    throw new InvalidOperationException(string.Format("The supplied filter '{0}' does not meet the required format for a filter +-[assemblyname]classname", assemblyClassName));
            }
            else
            {
                throw new InvalidOperationException(string.Format("The supplied filter '{0}' does not meet the required format for a filter +-[assemblyname]classname", assemblyClassName));
            }
        }

        public void AddAttributeExclusionFilters(string[] exclusionFilters)
        {
            if (exclusionFilters == null)
                return;
            foreach (var exlusionFilter in exclusionFilters.Where(x => x != null))
            {
                var filter = exlusionFilter;
                if (!RegExFilters)
                    filter = filter.ValidateAndEscape().WrapWithAnchors();
                ExcludedAttributes.Add(new Lazy<Regex>(() => new Regex(filter)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ExcludeByAttribute(IMemberDefinition entity)
        {
            while (true)
            {
                if (ExcludedAttributes.Count == 0)
                    return false;

                if (entity == null || !entity.HasCustomAttributes)
                    return false;

                if ((from excludeAttribute in ExcludedAttributes from customAttribute in entity.CustomAttributes where excludeAttribute.Value.Match(customAttribute.AttributeType.FullName).Success select excludeAttribute).Any())
                {
                    return true;
                }

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

        public bool ExcludeByFile(string fileName)
        {
            if (ExcludedFiles.Count == 0 || string.IsNullOrWhiteSpace(fileName))
                return false;

            return ExcludedFiles.Any(excludeFile => excludeFile.Value.Match(fileName).Success);
        }

        public void AddFileExclusionFilters(string[] exclusionFilters)
        {
            if (exclusionFilters == null)
                return;

            foreach (var exlusionFilter in exclusionFilters.Where(x => x != null))
            {
                var filter = exlusionFilter;
                if (!RegExFilters)
                    filter = filter.ValidateAndEscape(@"[]").WrapWithAnchors();

                ExcludedFiles.Add(new Lazy<Regex>(() => new Regex(filter)));
            }
        }

        public bool UseTestAssembly(string assemblyName)
        {
            if (TestFiles.Count == 0 || string.IsNullOrWhiteSpace(assemblyName))
                return false;

            return TestFiles.Any(file => file.Value.Match(assemblyName).Success);
        }

        public void AddTestFileFilters(string[] testFilters)
        {
            if (testFilters == null)
                return;

            foreach (var testFilter in testFilters.Where(x => x != null))
            {
                var filter = testFilter;
                if (!RegExFilters)
                    filter = filter.ValidateAndEscape(@"[]").WrapWithAnchors();

                TestFiles.Add(new Lazy<Regex>(() => new Regex(filter)));
            }
        }

        public bool IsAutoImplementedProperty(MethodDefinition method)
        {
            if ((method.IsSetter || method.IsGetter) && method.HasCustomAttributes)
            {
                return method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
            }
            return false;
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
