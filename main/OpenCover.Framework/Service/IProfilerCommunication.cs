using System.ServiceModel;

namespace OpenCover.Framework.Service
{
    [ServiceContract(Namespace = "urn:opencover.profiler")]
    public interface IProfilerCommunication
    {
        [OperationContract]
        bool Start();

        [OperationContract]
        bool Stop();
    }
}
