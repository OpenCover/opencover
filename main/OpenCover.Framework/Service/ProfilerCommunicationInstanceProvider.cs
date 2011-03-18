using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

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
        private readonly IUnityContainer _unityContainer;

        public ProfilerCommunicationInstanceProvider(
            IUnityContainer unityContainer,
            Type type)
        {
            _unityContainer = unityContainer;
            _type = type;
        }

        #region IInstanceProvider Members

        public object GetInstance(
            InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(
            InstanceContext instanceContext,
            Message message)
        {
            return _unityContainer.Resolve(_type);
        }

        public void ReleaseInstance(
            InstanceContext instanceContext,
            object instance)
        {
            _unityContainer.Teardown(instance);
        }

        #endregion
    }
}