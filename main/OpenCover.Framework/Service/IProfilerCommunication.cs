using System;
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;
using OpenCover.Framework.Common;

namespace OpenCover.Framework.Service
{

    [DataContract(Namespace = "urn:opencover.profiler")]
    public class VisitPoint
    {
        [DataMember]
        public VisitType VisitType { get; set; }
        [DataMember]
        public UInt32 UniqueId { get; set; }  
    }

    [DataContract(Namespace = "urn:opencover.profiler")]
    public class SequencePoint
    {
        [DataMember]
        public UInt32 Ordinal { get; set; }
        [DataMember]
        public UInt32 UniqueId { get; set; }
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
        bool GetSequencePoints(string moduleName, int functionToken, out SequencePoint[] sequencePoints);

        [OperationContract]
        void Visited(VisitPoint[] visitPoints);
    }
}
