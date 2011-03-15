using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace OpenCover.Framework.Service
{
    public class ProfilerCommunicationServiceHost : ServiceHost
    {
        public ProfilerCommunicationServiceHost(
            Type type, 
            params Uri[] baseAddresses) 
            : base(type, baseAddresses) { }

        protected override void OnOpening()
        {
            this.Description
                .Behaviors
                .Add(new ProfilerCommunicationServiceBehaviour());
            base.OnOpening();
        }
    }
}
