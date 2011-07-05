//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public interface IPersistance
    {
        void PersistModule(Module module);
        void Commit();
        bool GetSequencePointsForFunction(string moduleName, int functionToken, out SequencePoint[] sequencePoints);
        void SaveVisitPoints(VisitPoint[] visitPoints);
        bool IsTracking(string moduleName);
        string GetClassFullName(string moduleName, int functionToken);

        CoverageSession CoverageSession { get; }
    }
}