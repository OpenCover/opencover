using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Filtering
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

}
