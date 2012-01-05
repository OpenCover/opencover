using System;

namespace OpenCover.Framework
{
    /// <summary>
    /// This attribute can be used to hide code whilst testing
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    [ExcludeFromCoverage]
    internal class ExcludeFromCoverageAttribute : Attribute
    {}
}