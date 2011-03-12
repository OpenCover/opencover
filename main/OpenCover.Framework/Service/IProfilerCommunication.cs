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
        bool ShouldTrackAssembly(string assemblyName);

        [OperationContract]
        void Stop();
    }
}
