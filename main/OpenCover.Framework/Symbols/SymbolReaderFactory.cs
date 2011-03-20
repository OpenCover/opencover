using System;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

namespace OpenCover.Framework.Symbols
{
    public class SymbolReaderFactory : ISymbolReaderFactory
    {
        private readonly SymBinder _symBinder;

        public SymbolReaderFactory()
        {
            _symBinder = new SymBinder();
        }

        public ISymbolReader GetSymbolReader(string moduleName, string searchPath)
        {
            return GetSymbolReader(_symBinder, moduleName, searchPath);
        }

        // http://msdn.microsoft.com/en-us/library/ms686615(VS.85).aspx
        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(
            [In] ref Guid rclsid,
            [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter,
            [In] uint dwClsContext,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out Object ppv);

        // http://msdn.microsoft.com/en-us/library/ms232122.aspx
        [ComImport, Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMetaDataDispenser
        {
            uint DefineScope([In] ref Guid rclsid,
                [In] uint dwCreateFlags,
                [In] ref Guid riid,
                [Out, MarshalAs(UnmanagedType.Interface)]out object ppIUnk);

            uint OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] String szScope,
                [In] Int32 dwOpenFlags,
                [In] ref Guid riid,
                [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppIUnk);

            uint OpenScopeOnMemory([In] IntPtr pData,
                [In] uint cbData,
                [In] uint dwOpenFlags,
                [In] ref Guid riid,
                [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
        }

        // a stub - see http://msdn.microsoft.com/en-us/library/ms230172.aspx
        [ComImport, Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMetadataImport
        {
        }

        static Guid CLSID_CorMetaDataDispenser = new Guid(0xe5cb7a31, 0x7512, 0x11d2, 0x89, 0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8);
        static Guid IID_IMetaDataDispenser = new Guid(0x809c652e, 0x7396, 0x11d2, 0x97, 0x71, 0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c);
        static Guid IID_IMetaDataImport = new Guid(0x7dac8207, 0xd3ae, 0x4c75, 0x9b, 0x67, 0x92, 0x80, 0x1a, 0x49, 0x7d, 0x44);

        [System.Security.Permissions.SecurityPermission(
            System.Security.Permissions.SecurityAction.Demand,
            Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        public static ISymbolReader GetSymbolReader(SymBinder binder, string pathModule, string searchPath)
        {
            try
            {

                object objDispenser;
                CoCreateInstance(ref CLSID_CorMetaDataDispenser,
                                    null,
                                    1,
                                    ref IID_IMetaDataDispenser,
                                    out objDispenser);

                object objImporter;
                var dispenser = (IMetaDataDispenser) objDispenser;
                dispenser.OpenScope(pathModule, 0, ref IID_IMetaDataImport, out objImporter);

                var importerPtr = IntPtr.Zero;
                ISymbolReader reader;
                try
                {
                    importerPtr = Marshal.GetComInterfaceForObject(objImporter, typeof (IMetadataImport));

                    reader = binder.GetReader(importerPtr, pathModule, searchPath);
                }
                finally
                {
                    if (importerPtr != IntPtr.Zero)
                    {
                        Marshal.Release(importerPtr);
                    }
                }
                return reader;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

