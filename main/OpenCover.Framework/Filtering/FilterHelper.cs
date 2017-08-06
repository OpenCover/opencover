using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.Framework.Filtering
{
    internal static class FilterHelper
    {
        internal static string WrapWithAnchors(this string data)
        {
            return String.Format("^({0})$", data);
        }

        internal static IList<AssemblyAndClassFilter> GetMatchingFiltersForAssemblyName(this IEnumerable<AssemblyAndClassFilter> filters, string assemblyName)
        {
            var matchingFilters = filters
                .Where(filter => filter.IsMatchingAssemblyName(assemblyName)).ToList();
            return matchingFilters;
        }

        internal static IList<AssemblyAndClassFilter> GetMatchingFiltersForProcessName(this IEnumerable<AssemblyAndClassFilter> filters, string processName)
        {
            var matchingFilters = filters
                .Where(filter => filter.IsMatchingProcessName(processName)).ToList();
            return matchingFilters;
        }

        internal static void AddRange<T> (this ICollection<T> collection, IEnumerable<T> range) {
            if (collection != null && range != null) {
                foreach (var item in range)
                {
                    collection.Add(item);
                }
            }
        }

        
    }
}
