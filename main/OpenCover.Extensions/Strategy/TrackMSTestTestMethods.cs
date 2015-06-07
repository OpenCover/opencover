namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track MSTest test methods
    /// </summary>
    public class TrackMSTestTestMethods : TrackedMethodStrategyBase
    {
        private const string MsTestStrategyName = "MSTestTest";
        private const string MsTestAttributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";

        public TrackMSTestTestMethods()
            : base(MsTestStrategyName, MsTestAttributeName)
        {
        }
    }
}