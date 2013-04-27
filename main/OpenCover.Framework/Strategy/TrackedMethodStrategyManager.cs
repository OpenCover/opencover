using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Autofac.Configuration;
using Mono.Cecil;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Strategy
{
    /// <summary>
    /// Run strategies in a limited permissions AppDomain
    /// </summary>
    public class TrackedMethodStrategyManager : ITrackedMethodStrategyManager
    {
        private AppDomain _domain;
        private TrackedMethodStrategyProxy _proxy;

        private class TrackedMethodStrategyProxy : MarshalByRefObject
        {
            private readonly IEnumerable<ITrackedMethodStrategy> _strategies;

            public TrackedMethodStrategyProxy()
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new ConfigurationSettingsReader());
                var container = builder.Build();
                _strategies = container.Resolve<IEnumerable<ITrackedMethodStrategy>>();
            }

            public TrackedMethod[] GetTrackedMethods(string assembly)
            {
                var definition = AssemblyDefinition.ReadAssembly(assembly);
                if (definition == null) return null;
                
                var trackedmethods = new List<TrackedMethod>();
                foreach (var trackedMethodStrategy in _strategies)
                {
                    IEnumerable<TypeDefinition> typeDefinitions = definition.MainModule.Types;
                    trackedmethods.AddRange(trackedMethodStrategy.GetTrackedMethods(typeDefinitions));
                }
                return trackedmethods.ToArray();
            }

            public override object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary>
        /// Instantiate an instance of the manager that will execute the strategies  
        /// </summary>
        public TrackedMethodStrategyManager()
        {
            var info = new AppDomainSetup
                {
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                };

            _domain = AppDomain.CreateDomain("StrategySandBox", null, info);

            _proxy = (TrackedMethodStrategyProxy)_domain.CreateInstanceAndUnwrap(typeof(TrackedMethodStrategyProxy).Assembly.FullName,
                                                                                 typeof (TrackedMethodStrategyProxy).FullName);
        }

        private int _methodId;
        public TrackedMethod[] GetTrackedMethods(string assembly)
        {
            var methods =  _proxy.GetTrackedMethods(assembly);
            foreach (var trackedMethod in methods)
            {
                trackedMethod.UniqueId = (uint) Interlocked.Increment(ref _methodId);
            }
            return methods;
        }

        public void Dispose()
        {
            _proxy = null;
            if (_domain == null) return;
            try
            {
                AppDomain.Unload(_domain);
            }
            finally
            {
                _domain = null;
            }
        }
    }
}