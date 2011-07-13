//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        /// 
        /// </param>
        void AddFilter(string assemblyClassName);

        /// <summary>
        /// Decides whether an assembly should be included in the instrumentation
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <returns>the name of the class under profile</returns>
        /// <remarks>All assemblies matching either the inclusion or exclusion filter should be included 
        /// as it is the class that is being filtered within these unless the class filter is *</remarks>
        bool UseAssembly(string assemblyName);

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        bool InstrumentClass(string assemblyName, string className);
    }  

    internal static class FilterHelper
    {
        internal static string WrapWithAnchors(this string data)
        {
            return string.Format("^{0}$", data);
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
        internal IList<KeyValuePair<string, string>> InclusionFilter { get; set;}
        internal IList<KeyValuePair<string, string>> ExclusionFilter { get; set;}

        /// <summary>
        /// Standard constructor
        /// </summary>
        public Filter()
        {
            InclusionFilter = new List<KeyValuePair<string, string>>();
            ExclusionFilter = new List<KeyValuePair<string, string>>();
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
            if (string.IsNullOrEmpty(assemblyName)) return false;
            if (string.IsNullOrEmpty(className)) return false;

            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value == ".*"))
            {
                return false;
            }

            if (ExclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value != ".*")
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
            {
                Debug.WriteLine("Rejecting {0} {1}", assemblyName, className);
                return false;
            }

            if (InclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success)
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
            {
                return true;
            }

            Debug.WriteLine("Rejecting {0} {1}", assemblyName, className);
            return false;
        }

        public void AddFilter(string assemblyClassName)
        {
            string assemblyName;
            string className;
            FilterType filterType;
            GetAssemblyClassName(assemblyClassName, out filterType, out assemblyName, out className);

            assemblyName = ValidateAndEscape(assemblyName);
            className = ValidateAndEscape(className);

            if (filterType == FilterType.Inclusion) InclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));

            if (filterType == FilterType.Exclusion) ExclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));
        }

        private static void GetAssemblyClassName(string assemblyClassName, out FilterType filterType, out string assemblyName, out string className)
        {
            className = string.Empty;
            assemblyName = string.Empty;
            var regEx = new Regex(@"^(?<type>([+-]))(\[(?<assembly>(.+))\])(?<class>(.*))?$");
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

        private static string ValidateAndEscape(string match)
        {
            if (match.IndexOfAny(@"\[]".ToCharArray())>=0) throw new InvalidOperationException(string.Format("The string is invalid for an assembly/class name {0}", match));
            match = match.Replace(@".", @"\.");
            match = match.Replace(@"*", @".*");
            return match;
        }
    }
}
