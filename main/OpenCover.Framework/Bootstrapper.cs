using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
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

        public void Initialise(IFilter filter, ICommandLine commandLine)
        {
            _container.RegisterType<IProfilerCommunication, ProfilerCommunication>();
            _container.RegisterType<ISymbolManagerFactory, SymbolManagerFactory>();
            _container.RegisterType<ISymbolReaderFactory, SymbolReaderFactory>();
            _container.RegisterInstance(filter);
            _container.RegisterInstance(commandLine);
        }

    }
}
