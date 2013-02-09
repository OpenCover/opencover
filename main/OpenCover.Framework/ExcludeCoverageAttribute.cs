using System;

namespace OpenCover.Framework
{
    /// <summary>
    /// This attribute can be used to hide code whilst testing
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Constructor)]
    [ExcludeFromCoverage("It is an attribute and is not actually executed directly by a test but is used to hide code from coverage")]
    internal class ExcludeFromCoverageAttribute : Attribute
    {
        public string Reason { get; private set; }
        public ExcludeFromCoverageAttribute(string reason)
        {
            Reason = reason;
        }
    }
}