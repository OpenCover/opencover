using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunication : IProfilerCommunication
    {
        private readonly IFilter _filter;
        private readonly IPersistance _persistance;
        private readonly IInstrumentationModelBuilderFactory _instrumentationModelBuilderFactory;

        public CoverageSession CoverageSession { get; set; }

        public ProfilerCommunication(IFilter filter, 
            IPersistance persistance,
            IInstrumentationModelBuilderFactory instrumentationModelBuilderFactory)
        {
            _filter = filter;
            _persistance = persistance;
            _instrumentationModelBuilderFactory = instrumentationModelBuilderFactory;
        }

        public void Started()
        {
           
        }

        public bool TrackAssembly(string moduleName, string assemblyName)
        {
            var ret = _filter.UseAssembly(assemblyName);
            if (!ret) return false;
            var builder = _instrumentationModelBuilderFactory.CreateModelBuilder(moduleName);
            _persistance.PersistModule(builder.BuildModuleModel());
            return true;
        }

        public void Stopping()
        {
            _persistance.Commit();
        }
    }
}