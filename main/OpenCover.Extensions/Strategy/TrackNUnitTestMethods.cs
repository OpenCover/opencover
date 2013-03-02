using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;

namespace OpenCover.Extensions.Strategy
{
    /// <summary>
    /// Track NUnit test methods
    /// </summary>
    public class TrackNUnitTestMethods : ITrackedMethodStrategy
    {
        public IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where customAttribute.AttributeType.FullName == "NUnit.Framework.TestAttribute"
                    select new TrackedMethod()
                    {
                        MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                        Name = methodDefinition.FullName,
                        Strategy = "NUnitTest"
                    });
        }
    }
}
