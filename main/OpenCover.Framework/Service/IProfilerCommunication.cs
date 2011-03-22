using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace OpenCover.Framework.Service
{
    [DataContract(Namespace = "urn:opencover.profiler")]
    public class InstrumentPoint
    {
        [DataMember]
        public int Ordinal { get; set; }
        [DataMember]
        public long UniqueId { get; set; }
        [DataMember]
        public int Offset { get; set; }
    }

    [ServiceContract(Namespace = "urn:opencover.profiler", ProtectionLevel = ProtectionLevel.None)]
    public interface IProfilerCommunication
    {
        [OperationContract]
        void Started();

        [OperationContract]
        void Stopping();

        [OperationContract]
        bool TrackAssembly(string moduleName, string assemblyName);

        [OperationContract]
        bool GetSequencePoints(string moduleName, int functionToken, out InstrumentPoint[] sequencePoints);

    }
}
