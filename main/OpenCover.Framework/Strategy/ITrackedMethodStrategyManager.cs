using System;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Strategy
{
    /// <summary>
    /// Simple abstraction to load the potentially external 'strategies'
    /// in a limited permissions AppDomain 
    /// </summary>
    public interface ITrackedMethodStrategyManager : IDisposable
    {
        /// <summary>
        /// Get the tracked methods for the target assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        TrackedMethod[] GetTrackedMethods(string assembly);
    }
}
