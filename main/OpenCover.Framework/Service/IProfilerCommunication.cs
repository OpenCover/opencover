//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Net.Security;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Service
{
    //public class SequencePoint
    //{
    //    public UInt32 Ordinal { get; set; }
    //    public UInt32 UniqueId { get; set; }
    //    public int Offset { get; set; }
    //}

    //public class BranchPoint
    //{
    //    public UInt32 Ordinal { get; set; }
    //    public UInt32 UniqueId { get; set; }
    //    public int Offset { get; set; }
    //    public int Path { get; set; }
    //}

    public interface IProfilerCommunication
    {
        bool TrackAssembly(string modulePath, string assemblyName);

        bool GetSequencePoints(string modulePath, string assemblyName, int functionToken, out InstrumentationPoint[] sequencePoints);

        bool GetBranchPoints(string modulePath, string assemblyName, int functionToken, out BranchPoint[] branchPoints);

        void Stopping();
    }
}
