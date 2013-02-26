using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Strategy
{
    /// <summary>
    /// Track xUnit test methods
    /// </summary>
    public class TrackXUnitTestMethods : ITrackedMethodStrategy
    {
        public IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where customAttribute.AttributeType.FullName == "Xunit.FactAttribute"
                    select new TrackedMethod()
                        {
                            MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                            Name = methodDefinition.FullName,
                            Strategy = "xUnitTest"
                        });
        }
    }
}