//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Mono.Cecil;
using OpenCover.Framework.Filtering;

namespace OpenCover.Framework
{
    /// <summary>
    ///  A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public class Filter : IFilter
    {
        internal IList<AssemblyAndClassFilter> InclusionFilters { get; private set; }
        internal IList<AssemblyAndClassFilter> ExclusionFilters { get; private set; }
        internal IList<RegexFilter> ExcludedAttributes { get; private set; }
        internal IList<RegexFilter> ExcludedFiles { get; private set; }
        internal IList<RegexFilter> TestFiles { get; private set; }
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
                    throw InvalidFilterFormatException(assemblyClassName);
            }
            else
            {
                throw InvalidFilterFormatException(assemblyClassName);
            }
        }

        private static InvalidOperationException InvalidFilterFormatException(string assemblyClassName)
        {
            return new InvalidOperationException(string.Format("The supplied filter '{0}' does not meet the required format for a filter +-[assemblyname]classname", assemblyClassName));
        }

        public void AddAttributeExclusionFilters(string[] exclusionFilters)
        {
            ExcludedAttributes.AddFilters(exclusionFilters, RegExFilters);
        }

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

        public bool ExcludeByAttribute(AssemblyDefinition entity)
        {
            if (ExcludedAttributes.Count == 0)
                return false;

            return ExcludeByAttribute((ICustomAttributeProvider)entity);
        }

        public bool ExcludeByFile(string fileName)
        {
            if (ExcludedFiles.Count == 0 || string.IsNullOrWhiteSpace(fileName))
                return false;

            return ExcludedFiles.Any(excludeFile => excludeFile.IsMatchingExpression(fileName));
        }

        public void AddFileExclusionFilters(string[] exclusionFilters)
        {
            ExcludedFiles.AddFilters(exclusionFilters, RegExFilters);
        }

        public bool UseTestAssembly(string assemblyName)
        {
            if (TestFiles.Count == 0 || string.IsNullOrWhiteSpace(assemblyName))
                return false;

            return TestFiles.Any(file => file.IsMatchingExpression(assemblyName));
        }

        public void AddTestFileFilters(string[] testFilters)
        {
            TestFiles.AddFilters(testFilters, RegExFilters);
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
