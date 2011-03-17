using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenCover.Framework.Model;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunication : IProfilerCommunication
    {
        private readonly IFilter _filter;
        private readonly ISymbolManagerFactory _symbolManagerFactory;
        private readonly ISymbolReaderFactory _symbolReaderFactory;

        public CoverageSession CoverageSession { get; set; }

        public ProfilerCommunication(IFilter filter, ISymbolManagerFactory symbolManagerFactory, ISymbolReaderFactory symbolReaderFactory)
        {
            _filter = filter;
            _symbolManagerFactory = symbolManagerFactory;
            _symbolReaderFactory = symbolReaderFactory;
        }

        public void Started()
        {
            CoverageSession = new CoverageSession();
        }

        public bool ShouldTrackAssembly(string moduleName, string assemblyName)
        {
            var ret = _filter.UseAssembly(assemblyName);
            if (!ret) return false;
            var manager = _symbolManagerFactory.CreateSymbolManager(moduleName, Path.GetDirectoryName(moduleName), _symbolReaderFactory);
            var builder = new InstrumentationModelBuilder(manager);
            var list = new List<Module>(CoverageSession.Modules ?? new Module[0]);
            list.Add(builder.BuildModuleModel());
            CoverageSession.Modules = list.ToArray();
            return true;
        }

        public void Stopping()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                var fs = new FileStream("temp.xml", FileMode.Create);
                var writer = new StreamWriter(fs, new UTF8Encoding());
                serializer.Serialize(writer, CoverageSession);
                writer.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            
            
        }
    }
}