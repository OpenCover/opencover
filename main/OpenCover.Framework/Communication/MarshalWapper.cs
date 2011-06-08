using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenCover.Framework.Communication
{
    public interface IMarshalWrapper
    {
        T PtrToStructure<T>(IntPtr pinnedMemory);
        void StructureToPtr<T>(T structure, IntPtr pinnedMemory, bool fDeleteOld);
    }

    public class MarshalWrapper : IMarshalWrapper
    {
        public T PtrToStructure<T>(IntPtr pinnedMemory)
        {
            return (T)Marshal.PtrToStructure(pinnedMemory, typeof(T));
        }

        public void StructureToPtr<T>(T structure, IntPtr pinnedMemory, bool fDeleteOld)
        {
            Marshal.StructureToPtr(structure, pinnedMemory, fDeleteOld);
        }
    }
}
