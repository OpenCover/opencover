using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Service
{
    /// <summary>
    /// This Provider allows us to return a more complex
    /// constructed Profiler Communication object
    /// </summary>
    public class ProfilerCommunicationInstanceProvider 
        : IInstanceProvider
    {
        private readonly Type _type;
        public ProfilerCommunicationInstanceProvider(
            Type type)
        {
            _type = type;
        }

        public object GetInstance(
            InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(
            InstanceContext instanceContext, 
            Message message)
        {
            var filter = new Filter();
            filter.AddFilter("-[mscorlib]*");
            filter.AddFilter("-[System.*]*");
            filter.AddFilter("+[*]*");
            return _type == typeof(ProfilerCommunication) ? new ProfilerCommunication(filter, new SymbolManagerFactory(), new SymbolReaderFactory()) : _type.Assembly.CreateInstance(_type.FullName);
        }

        public void ReleaseInstance(
            InstanceContext instanceContext, 
            object instance) { }
    }
}
