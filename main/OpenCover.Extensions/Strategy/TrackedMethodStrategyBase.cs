using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;

namespace OpenCover.Extensions.Strategy
{
    public abstract class TrackedMethodStrategyBase : ITrackedMethodStrategy
    {
        

        protected static IEnumerable<TrackedMethod> GetTrackedMethodsByAttribute(IEnumerable<TypeDefinition> typeDefinitions, string attributeName,
                                                                                 string strategyName)
        {
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where customAttribute.AttributeType.FullName == attributeName
                    select new TrackedMethod()
                        {
                            MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                            Name = methodDefinition.FullName,
                            Strategy = strategyName
                        });
        }

        public abstract IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions);
    }
}