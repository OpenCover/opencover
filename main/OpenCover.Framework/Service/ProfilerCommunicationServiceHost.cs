using System;
using System.ServiceModel;
using Microsoft.Practices.Unity;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunicationServiceHost : ServiceHost
    {
        private readonly IUnityContainer _unityContainer;

        public ProfilerCommunicationServiceHost(IUnityContainer unityContainer,
                                                Type type, params Uri[] baseAddresses)
            : base(type, baseAddresses)
        {
            _unityContainer = unityContainer;
        }

        protected override void OnOpening()
        {
            Description.Behaviors
                .Add(new ProfilerCommunicationServiceBehaviour(_unityContainer));

            base.OnOpening();
        }
    }
}