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

        internal static string ValidateAndEscape(this string match, string notAllowed = @"\[]")
        {
            if (match.IndexOfAny(notAllowed.ToCharArray()) >= 0) throw new InvalidOperationException(String.Format("The string is invalid for an filter name {0}", match));
            match = match.Replace(@"\", @"\\");
            match = match.Replace(@".", @"\.");
            match = match.Replace(@"*", @".*");
            return match;
        }

        internal static IList<AssemblyAndClassFilter> GetMatchingFiltersForAssemblyName(this IEnumerable<AssemblyAndClassFilter> filters, string assemblyName)
        {
            var matchingFilters =
                filters.Where(exclusionFilter => exclusionFilter.IsMatchingAssemblyName(assemblyName)).ToList();
            return matchingFilters;
        }
    }
}
