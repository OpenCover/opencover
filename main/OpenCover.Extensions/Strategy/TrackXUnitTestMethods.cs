using System.Collections.Generic;

namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track xUnit test methods
    /// </summary>
    public class TrackXUnitTestMethods : TrackedMethodStrategyBase
    {
        private const string XUnitStrategyName = "xUnitTest";

        private static readonly IList<string> TrackedAttributeTypeNames = new List<string>
        {
            "Xunit.FactAttribute",
            "Xunit.TheoryAttribute",
        };

        public TrackXUnitTestMethods() : base(XUnitStrategyName, TrackedAttributeTypeNames)
        {                
        }
    }
}