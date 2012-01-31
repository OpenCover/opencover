using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Strategy
{
    /// <summary>
    /// Expose how a method is tracked 
    /// </summary>
    public interface ITrackedMethodStrategy
    {
        /// <summary>
        /// Return a list of methods that are to be tracked
        /// </summary>
        /// <param name="typeDefinitions">A list of type definitions (uses Mono.Cecil)</param>
        /// <returns></returns>
        IEnumerable<TrackedMethod> GetTrackedMethods(IEnumerable<TypeDefinition> typeDefinitions);
    }
}
