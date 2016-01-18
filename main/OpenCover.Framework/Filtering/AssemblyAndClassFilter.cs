using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Filtering
{
    internal class AssemblyAndClassFilter
    {
        private readonly RegexFilter _processFilter;

        private readonly RegexFilter _assemblyFilter;

        private readonly RegexFilter _classFilter;

        internal string ProcessName { get { return _processFilter.FilterExpression; } }

        internal string AssemblyName { get { return _assemblyFilter.FilterExpression; } }

        internal string ClassName { get { return _classFilter.FilterExpression; } }

        internal AssemblyAndClassFilter(string processFilter, string assemblyFilter, string classFilter)
        {
            _processFilter = new RegexFilter(processFilter);
            _assemblyFilter = new RegexFilter(assemblyFilter);
            _classFilter = new RegexFilter(classFilter);
        }

        internal bool IsMatchingProcessName(string processPathOrName)
        {
            return _processFilter.IsMatchingExpression(processPathOrName);
        }

        internal bool IsMatchingAssemblyName(string assemblyPathOrName)
        {
            return _assemblyFilter.IsMatchingExpression(assemblyPathOrName);
        }

        internal bool IsMatchingClassName(string className)
        {
            return _classFilter.IsMatchingExpression(className);
        }
    }

}
