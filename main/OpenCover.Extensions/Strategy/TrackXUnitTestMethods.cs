namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track xUnit test methods
    /// </summary>
    public class TrackXUnitTestMethods : TrackedMethodStrategyBase
    {
        public TrackXUnitTestMethods() : base("xUnitTest", "Xunit.FactAttribute")
        {                
        }
    }
}