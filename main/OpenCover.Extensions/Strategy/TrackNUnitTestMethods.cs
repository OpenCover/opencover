using System.Collections.Generic;

namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track NUnit test methods
    /// </summary>
    public class TrackNUnitTestMethods : TrackedMethodStrategyBase
    {
        private const string NUnitStrategyName = "NUnitTest";

        private static readonly IList<string> TrackedAttributeTypeNames = new List<string>
        {
            "NUnit.Framework.TestAttribute",
            "NUnit.Framework.TestCaseAttribute",
            "NUnit.Framework.TheoryAttribute"
        };

        public TrackNUnitTestMethods() : base(NUnitStrategyName, TrackedAttributeTypeNames)
        {            
        }
    }
}
