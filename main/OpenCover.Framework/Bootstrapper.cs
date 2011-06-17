//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework
{
    public class Bootstrapper
    {
        private readonly IUnityContainer _container;

        public Bootstrapper()
        {
            _container = new UnityContainer();
        }

        public IUnityContainer Container
        {
            get { return _container; }
        }

        public void Initialise(IFilter filter, ICommandLine commandLine, IPersistance persistance)
        {
            _container.RegisterType<IProfilerCommunication, ProfilerCommunication>();
            _container.RegisterType<ISymbolManagerFactory, CecilSymbolManagerFactory>();
            _container.RegisterType<IInstrumentationModelBuilderFactory, InstrumentationModelBuilderFactory>();
            _container.RegisterInstance(filter);
            _container.RegisterInstance(commandLine);
            _container.RegisterInstance(persistance);
            _container.RegisterType<IInstrumentationModelBuilderFactory, InstrumentationModelBuilderFactory>();
            _container.RegisterType<IProfilerManager, ProfilerManager>();
            _container.RegisterType<IMessageHandler, MessageHandler>();
            _container.RegisterType<IMarshalWrapper, MarshalWrapper>();
        }

    }
}
