using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using OpenCover.Framework.Model;
using OpenCover.Framework.Strategy;

namespace OpenCover.Extensions.Strategy
{
    public abstract class TrackedMethodStrategyBase : ITrackedMethodStrategy
    {
        private readonly ISet<string> _acceptedAttributes;

        public string StrategyName { get; }

        protected TrackedMethodStrategyBase(string strategyName, IEnumerable<string> attributeNames)
        {
            StrategyName = strategyName;
            _acceptedAttributes = new HashSet<string>(attributeNames);
        }

        protected TrackedMethodStrategyBase(string strategyName, string attribute)
        {
            StrategyName = strategyName;
            _acceptedAttributes = new HashSet<string> { attribute };
        }

        protected IEnumerable<TrackedMethod> GetTrackedMethodsByAttribute(IEnumerable<TypeDefinition> typeDefinitions)
        {
            return (from typeDefinition in typeDefinitions
                from methodDefinition in typeDefinition.Methods
                from customAttribute in methodDefinition.CustomAttributes
                where _acceptedAttributes.Contains(customAttribute.AttributeType.FullName)
                select new TrackedMethod
                {
                    MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                    FullName = methodDefinition.FullName,
                    Strategy = StrategyName
                });
        }

        public IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions)
        {
            var allTypes = GetAllTypes(typeDefinitions);

            return GetTrackedMethodsByAttribute(allTypes);
        }

        private static IEnumerable<TypeDefinition> GetAllTypes(IEnumerable<TypeDefinition> typeDefinitions)
        {
            var types = new List<TypeDefinition>();
            foreach (var typeDefinition in typeDefinitions)
            {
                types.Add(typeDefinition);
                if (typeDefinition.HasNestedTypes)
                {
                    types.AddRange(GetAllTypes(typeDefinition.NestedTypes));
                }
            }
            return types;
        }
    }
}