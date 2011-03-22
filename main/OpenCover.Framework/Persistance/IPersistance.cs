using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public interface IPersistance
    {
        void PersistModule(Module module);
        void Commit();
        bool GetSequencePointsForFunction(string moduleName, int functionToken, out SequencePoint[] sequencePoints);
    }
}