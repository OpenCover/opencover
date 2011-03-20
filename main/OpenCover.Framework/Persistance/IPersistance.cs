using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    public interface IPersistance
    {
        void PersistModule(Module module);
        void Commit();
    }
}