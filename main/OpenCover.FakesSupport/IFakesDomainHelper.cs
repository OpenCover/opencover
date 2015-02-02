using System.Runtime.InteropServices;

namespace OpenCover.FakesSupport
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("2180EC45-CF11-456E-9A76-389A4521A4BE")]
    [ComVisible(true)]
    public interface IFakesDomainHelper
    {
        [ComVisible(true)]
        void AddResolveEventHandler();
    }
}