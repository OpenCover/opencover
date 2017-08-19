namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track MSTest test methods
    /// </summary>
    public class TrackMSTestTestMethods : TrackedMethodStrategyBase
    {
        private const string MSTestStrategyName = "MSTestTest";
        private const string MSTestAttributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";

        public TrackMSTestTestMethods()
            : base(MSTestStrategyName, MSTestAttributeName)
        {
        }
    }
}