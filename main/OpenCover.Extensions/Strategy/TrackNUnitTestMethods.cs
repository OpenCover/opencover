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
        private static readonly ISet<string> TrackedAttributeTypeNames = new HashSet<string>
        {
            "NUnit.Framework.TestAttribute",
            "NUnit.Framework.TestCaseAttribute",
            "NUnit.Framework.TheoryAttribute"
        };

        public IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where TrackedAttributeTypeNames.Contains(customAttribute.AttributeType.FullName)
                    select new TrackedMethod()
                    {
                        MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                        Name = methodDefinition.FullName,
                        Strategy = "NUnitTest"
                    });
        }
    }
}
