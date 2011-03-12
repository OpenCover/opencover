using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenCover.Framework
{
    public interface IFilter
    {
        void AddFilter(string assemblyClassName);
        bool UseAssembly(string assemblyName);
    }

    public enum FilterType
    {
        Inclusion,
        Exclusion
    }

    public class Filter : IFilter
    {
        public IList<KeyValuePair<string, string>> InclusionFilter { get; set;}
        public IList<KeyValuePair<string, string>> ExclusionFilter { get; set;}

        public Filter()
        {
            InclusionFilter = new List<KeyValuePair<string, string>>();
            ExclusionFilter = new List<KeyValuePair<string, string>>();
        }

        public bool UseAssembly(string assemblyName)
        {
            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key).Success))
            {
                return false;
            }

            if (InclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key).Success))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a filter
        /// </summary>
        /// <param name="assemblyClassName">A filter is of the format (+ or -)[assemblyName]className, wildcards are allowed. <br/>
        /// i.e. -[mscorlib], -[System.*]*, +[App.*]*, +[*]*
        /// 
        /// </param>
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

        /// <summary>
        /// Extracts the type of filter and assembly/class (optional) pair
        /// </summary>
        /// <param name="assemblyClassName"></param>
        /// <param name="filterType"></param>
        /// <param name="assemblyName"></param>
        /// <param name="className"></param>
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

        /// <summary>
        /// Validates the assembly class format and then escapes it for future regex usage
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string ValidateAndEscape(string match)
        {
            if (match.IndexOfAny(@"\[]".ToCharArray())>=0) throw new InvalidOperationException(string.Format("The string is invalid for an assembly/class name {0}", match));
            match = match.Replace(@".", @"\.");
            match = match.Replace(@"*", @".*");
            return match;
        }
    }
}
