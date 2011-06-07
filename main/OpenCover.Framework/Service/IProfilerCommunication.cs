using System;
using System.Net.Security;
using OpenCover.Framework.Common;

namespace OpenCover.Framework.Service
{

    public class VisitPoint
    {
        public VisitType VisitType { get; set; }
        public UInt32 UniqueId { get; set; }  
    }

   public class SequencePoint
    {
        public UInt32 Ordinal { get; set; }
        public UInt32 UniqueId { get; set; }
        public int Offset { get; set; }
    }

    public interface IProfilerCommunication
    {
        bool TrackAssembly(string moduleName, string assemblyName);

        bool GetSequencePoints(string moduleName, int functionToken, out SequencePoint[] sequencePoints);

        void Visited(VisitPoint[] visitPoints);
    }
}
