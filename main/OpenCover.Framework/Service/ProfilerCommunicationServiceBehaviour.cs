using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunicationServiceBehaviour 
        : IServiceBehavior
    {
        public void Validate(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase) { }

        public void AddBindingParameters(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, 
            BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase)
        {
            foreach (var ed in serviceHostBase.ChannelDispatchers
                .OfType<ChannelDispatcher>().SelectMany(cd => cd.Endpoints))
            {
                ed.DispatchRuntime.InstanceProvider =
                    new ProfilerCommunicationInstanceProvider(serviceDescription.ServiceType);
            }
        }
    }
}