//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using OpenCover.Framework.Utility;
using log4net;

namespace OpenCover.Framework
{
    /// <summary>
    /// Wraps up the Dependancy Injection framework
    /// </summary>
    public class Bootstrapper
    {
        private readonly ILog _logger;
        private readonly IUnityContainer _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">the log4net logger to be used for logging</param>
        public Bootstrapper(ILog logger)
        {
            _logger = logger;
            _container = new UnityContainer();
        }

        /// <summary>
        /// Access the unity container
        /// </summary>
        public IUnityContainer Container
        {
            get { return _container; }
        }

        /// <summary>
        /// Initialise the bootstrapper
        /// </summary>
        /// <param name="filter">a series of filters</param>
        /// <param name="commandLine">command line options needed by other components</param>
        /// <param name="persistance">a persistence object</param>
        public void Initialise(IFilter filter,
                               ICommandLine commandLine,
                               IPersistance persistance,
                               IMemoryManager memoryManager,
                               IPerfCounters perfCounters)
        {
            _container.RegisterInstance(_logger);
            _container.RegisterInstance(filter);
            _container.RegisterInstance(commandLine);
            _container.RegisterInstance(persistance);
            _container.RegisterInstance(memoryManager);
            _container.RegisterInstance(perfCounters);
            _container.RegisterType<IInstrumentationModelBuilderFactory, InstrumentationModelBuilderFactory>();
            _container.RegisterType<IProfilerManager, ProfilerManager>();
            _container.RegisterType<IProfilerCommunication, ProfilerCommunication>();
            _container.RegisterType<IMessageHandler, MessageHandler>();
            _container.RegisterType<IMarshalWrapper, MarshalWrapper>();
            _container.RegisterType<ITrackedMethodStrategy, TrackNUnitTestMethods>(
                typeof (TrackNUnitTestMethods).FullName);
            _container.RegisterType<ITrackedMethodStrategy, TrackMSTestTestMethods>(
                typeof (TrackMSTestTestMethods).FullName);
        }

    }
}
