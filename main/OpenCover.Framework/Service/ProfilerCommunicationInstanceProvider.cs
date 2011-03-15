using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

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
            return _type == typeof(ProfilerCommunication) ? new ProfilerCommunication() : _type.Assembly.CreateInstance(_type.FullName);
        }

        public void ReleaseInstance(
            InstanceContext instanceContext, 
            object instance) { }
    }
}
