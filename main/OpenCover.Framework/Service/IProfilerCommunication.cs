using System.Net.Security;
using System.ServiceModel;

namespace OpenCover.Framework.Service
{

    [ServiceContract(Namespace = "urn:opencover.profiler", ProtectionLevel = ProtectionLevel.None)]
    public interface IProfilerCommunication
    {
        [OperationContract]
        void Start();

        [OperationContract]
        void Stop();
    }
}
