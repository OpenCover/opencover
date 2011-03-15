using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using NUnit.Framework;
using OpenCover.Framework.Service;

namespace OpenCover.Test.Framework
{
    /// <summary>
    /// The service classes are all tightly integrated and 
    /// Microsoft have seen fit to use sealed classes without 
    /// interfaces making mocking this code a nightmare 
    /// unless you used something like TypeMock
    /// </summary>
    /// <remarks>
    /// This one of the rare Integration tests created more 
    /// out of necessity
    /// </remarks>
    [TestFixture, Category("Integration")]
    public class ServiceTests
    {
        public class ProfilerCommunicationClient : ClientBase<IProfilerCommunication>
        {
            public ProfilerCommunicationClient(ServiceEndpoint endpoint) : base(endpoint) { }
        }

        private ProfilerServiceHost _host;

        [SetUp]
        public void Setup()
        {
            _host = new ProfilerServiceHost();
            _host.Open(8001);
        }

        [TearDown]
        public void Teardown()
        {
           _host.Close(); 
        }

        [Test]
        public void Can_Open_Socket_And_Connect()
        {
            // arrange
            var binding = new NetTcpBinding()
            {
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                Security = { Mode = SecurityMode.None }
            };

            var endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(IProfilerCommunication)),
                binding,
                new EndpointAddress(new Uri("net.tcp://localhost:8001/OpenCover.Profiler.Host")));
            
            // act/assert
            var client = new ProfilerCommunicationClient(endpoint);
            Assert.DoesNotThrow(client.Open);

            // cleanup
            client.Close();
        }
    }
}
