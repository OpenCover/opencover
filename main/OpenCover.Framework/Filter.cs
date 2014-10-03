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

namespace OpenCover.Framework
{
    /// <summary>
    /// A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Add a filter
        /// </summary>
        /// <param name="assemblyClassName">A filter is of the format (+ or -)[assemblyName]className, wildcards are allowed. <br/>
        /// i.e. -[mscorlib], -[System.*]*, +[App.*]*, +[*]*
        /// </param>
        void AddFilter(string assemblyClassName);

        /// <summary>
        /// Decides whether an assembly should be included in the instrumentation
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <remarks>All assemblies matching either the inclusion or exclusion filter should be included 
        /// as it is the class that is being filtered within these unless the class filter is *</remarks>
        bool UseAssembly(string assemblyName);

        /// <summary>
        /// Decides whether an assembly should be analysed for test methods
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <returns>true - if the assembly matches the test assembly filter</returns>
        bool UseTestAssembly(string assemblyName);

        /// <summary>
        /// Add file exclusion filters
        /// </summary>
        /// <param name="exclusionFilters"></param>
        void AddFileExclusionFilters(string[] exclusionFilters);

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        bool InstrumentClass(string assemblyName, string className);

        /// <summary>
        /// Add attribute exclusion filters
        /// </summary>
        /// <param name="exclusionFilters">An array of filters that are used to wildcard match an attribute</param>
        void AddAttributeExclusionFilters(string[] exclusionFilters);

        /// <summary>
        /// Is this entity excluded due to an attributeFilter
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <returns></returns>
        bool ExcludeByAttribute(IMemberDefinition entity);

        /// <summary>
        /// Is this file excluded
        /// </summary>
        /// <param name="fileName">The name of the file to test</param>
        /// <returns></returns>
        bool ExcludeByFile(string fileName);

        /// <summary>
        /// Add test file filters
        /// </summary>
        /// <param name="testFilters"></param>
        void AddTestFileFilters(string[] testFilters);

        /// <summary>
        /// Is the method an auto-implemented property get/set
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        bool IsAutoImplementedProperty(MethodDefinition method);

        /// <summary>
        /// filters should be treated as regular expressions rather than wildcard
        /// </summary>
        bool RegExFilters { get; set; }
    }  

    internal static class FilterHelper
    {
        internal static string WrapWithAnchors(this string data)
        {
            return String.Format("^({0})$", data);
        }

        internal static string ValidateAndEscape(this string match, string notAllowed = @"\[]")
        {
            if (match.IndexOfAny(notAllowed.ToCharArray()) >= 0) throw new InvalidOperationException(String.Format("The string is invalid for an filter name {0}", match));
            match = match.Replace(@"\", @"\\");
            match = match.Replace(@".", @"\.");
            match = match.Replace(@"*", @".*");
            return match;
        }
    }

    /// <summary>
    /// The type of filter, an exclusion filter takes precedence over inclusion filter
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// The filter is an inclusion type, i.e. if a assembly/class pair 
        /// matches the filter then it is included for instrumentation
        /// </summary>
        Inclusion,

        /// <summary>
        /// The filter is an exclusion type, i.e. if a assembly/class pair 
        /// matches the filter then it is excluded for instrumentation
        /// </summary>
        Exclusion
    }
    
    /// <summary>
    ///  A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public class Filter : IFilter
    {
        internal IList<KeyValuePair<string, string>> InclusionFilter { get; set; }
        internal IList<KeyValuePair<string, string>> ExclusionFilter { get; set; }
        internal IList<Lazy<Regex>> ExcludedAttributes { get; set; }
        internal IList<Lazy<Regex>> ExcludedFiles { get; set; }
        internal IList<Lazy<Regex>> TestFiles { get; set; }
        public bool RegExFilters { get; set; }

        /// <summary>
        /// Standard constructor
        /// </summary>
        public Filter()
        {
            InclusionFilter = new List<KeyValuePair<string, string>>();
            ExclusionFilter = new List<KeyValuePair<string, string>>();
            ExcludedAttributes = new List<Lazy<Regex>>();
            ExcludedFiles = new List<Lazy<Regex>>();
            TestFiles = new List<Lazy<Regex>>();
        }
        
        public bool UseAssembly(string assemblyName)
        {
            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value == ".*"))
            {
                return false;
            }

            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value != ".*"))
            {
                return true;
            }

            if (InclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success))
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

            if (ExclusionFilter
                .Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value == ".*"))
            {
                return false;
            }

            if (ExclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value != ".*")
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
            {
                return false;
            }

            if (InclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success)
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
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

            if (filterType == FilterType.Inclusion) 
                InclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));

            if (filterType == FilterType.Exclusion) 
                ExclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));
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
                filterType = match.Groups["type"].Value == "+" ? FilterType.Inclusion : FilterType.Exclusion;
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

                if (entity ==null || !entity.HasCustomAttributes)
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
    }
}
