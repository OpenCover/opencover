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
using Autofac.Builder;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Strategy;
using OpenCover.Framework.Symbols;
using OpenCover.Framework.Utility;
using log4net;
using Autofac;
using IContainer = Autofac.IContainer;

namespace OpenCover.Framework
{
    /// <summary>
    /// Wraps up the Dependancy Injection framework
    /// </summary>
    public class Bootstrapper : IDisposable
    {
        private readonly ILog _logger;
        private IContainer _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">the log4net logger to be used for logging</param>
        public Bootstrapper(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Access the container
        /// </summary>
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Initialise the bootstrapper
        /// </summary>
        /// <param name="filter">a series of filters</param>
        /// <param name="commandLine">command line options needed by other components</param>
        /// <param name="persistance">a persistence object</param>
        /// <param name="perfCounters"></param>
        public void Initialise(IFilter filter,
                               ICommandLine commandLine,
                               IPersistance persistance,
                               IPerfCounters perfCounters)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_logger);
            builder.RegisterInstance(filter);
            builder.RegisterInstance(commandLine);
            builder.RegisterInstance(persistance);
            builder.RegisterInstance(perfCounters);

            builder.RegisterType<InstrumentationModelBuilderFactory>().As<IInstrumentationModelBuilderFactory>();
            builder.RegisterType<CommunicationManager>().As<ICommunicationManager>().SingleInstance();
            builder.RegisterType<MessageHandler>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<ProfilerManager>().As<IProfilerManager>().SingleInstance();
            builder.RegisterType<ProfilerCommunication>().As<IProfilerCommunication>().SingleInstance();
            builder.RegisterType<MarshalWrapper>().As<IMarshalWrapper>().SingleInstance();
            builder.RegisterType<MemoryManager>().As<IMemoryManager>().SingleInstance();

            builder.RegisterType<TrackNUnitTestMethods>().As<ITrackedMethodStrategy>();
            builder.RegisterType<TrackMSTestTestMethods>().As<ITrackedMethodStrategy>();

            _container = builder.Build();
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
