using System.Runtime.InteropServices;

namespace OpenCover.Support
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("2180EC45-CF11-456E-9A76-389A4521A4BE")]
    [ComVisible(true)]
    public interface IDomainHelper
    {
        [ComVisible(true)]
        void AddResolveEventHandler();
    }
}