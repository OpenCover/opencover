using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Filtering
{
    internal class AssemblyAndClassFilter
    {
        private readonly RegexFilter _processNameFilter;

        private readonly RegexFilter _assemblyNameFilter;

        private readonly RegexFilter _classNameFilter;

        internal string ProcessName { get { return _processNameFilter.FilterExpression; } }

        internal string AssemblyName { get { return _assemblyNameFilter.FilterExpression; } }

        internal string ClassName { get { return _classNameFilter.FilterExpression; } }

        internal AssemblyAndClassFilter(string processName, string assemblyName, string className)
        {
            _processNameFilter = new RegexFilter(processName);
            _assemblyNameFilter = new RegexFilter(assemblyName);
            _classNameFilter = new RegexFilter(className);
        }

        internal bool IsMatchingProcessName(string processName)
        {
            return _processNameFilter.IsMatchingExpression(processName);
        }

        internal bool IsMatchingAssemblyName(string assemblyName)
        {
            return _assemblyNameFilter.IsMatchingExpression(assemblyName);
        }

        internal bool IsMatchingClassName(string className)
        {
            return _classNameFilter.IsMatchingExpression(className);
        }
    }

}
