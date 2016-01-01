using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;

namespace OpenCover.Extensions.Strategy
{
    public abstract class TrackedMethodStrategyBase : ITrackedMethodStrategy
    {
        private readonly ISet<string> acceptedAttributes;

        public string StrategyName { get; private set; }

        protected TrackedMethodStrategyBase(string strategyName, IEnumerable<string> attributeNames)
        {
            StrategyName = strategyName;
            acceptedAttributes = new HashSet<string>(attributeNames);
        }

        protected TrackedMethodStrategyBase(string strategyName, string attribute)
        {
            StrategyName = strategyName;
            acceptedAttributes = new HashSet<string> { attribute };
        }

        protected IEnumerable<TrackedMethod> GetTrackedMethodsByAttribute(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return (from typeDefinition in typeDefinitions
                    from methodDefinition in typeDefinition.Methods
                    from customAttribute in methodDefinition.CustomAttributes
                    where acceptedAttributes.Contains(customAttribute.AttributeType.FullName)
                    select new TrackedMethod
                    {
                        MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                        FullName = methodDefinition.FullName,
                        Strategy = StrategyName
                    });
        }

        public IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return GetTrackedMethodsByAttribute(typeDefinitions);
        }
    }
}