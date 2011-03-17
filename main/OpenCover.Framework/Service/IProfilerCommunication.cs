using System.Net.Security;
using System.ServiceModel;

namespace OpenCover.Framework.Service
{

    [ServiceContract(Namespace = "urn:opencover.profiler", ProtectionLevel = ProtectionLevel.None)]
    public interface IProfilerCommunication
    {
        [OperationContract]
        void Started();

        [OperationContract]
        bool ShouldTrackAssembly(string moduleName, string assemblyName);

        [OperationContract]
        void Stopping();
    }
}
