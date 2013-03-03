using System.Collections.Generic;
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track MSTest test methods
    /// </summary>
    public class TrackMSTestTestMethods : TrackedMethodStrategyBase
    {
        public override IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            const string attributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
            const string strategyName = "MSTestTest";

            return GetTrackedMethodsByAttribute(typeDefinitions, attributeName, strategyName);
        }

        
    }
}