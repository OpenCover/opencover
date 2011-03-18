using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunicationServiceBehaviour
        : IServiceBehavior
    {
        private readonly IUnityContainer _unityContainer;

        public ProfilerCommunicationServiceBehaviour(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        #region IServiceBehavior Members

        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            foreach (var endpointDispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>()
                    .SelectMany(cd => cd.Endpoints))
            {
                endpointDispatcher.DispatchRuntime.InstanceProvider =
                    new ProfilerCommunicationInstanceProvider(_unityContainer, serviceDescription.ServiceType);
            }
        }

        #endregion
    }
}