//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenCover.Framework.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMarshalWrapper
    {
        /// <summary>
        /// Map pinned memory to a structure
        /// </summary>
        /// <typeparam name="T">The type of the structure</typeparam>
        /// <param name="pinnedMemory"></param>
        /// <returns></returns>
        T PtrToStructure<T>(IntPtr pinnedMemory);

        /// <summary>
        /// Map a structure to pinned memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <param name="pinnedMemory"></param>
        /// <param name="fDeleteOld"></param>
        void StructureToPtr<T>(T structure, IntPtr pinnedMemory, bool fDeleteOld);
    }

    /// <summary>
    /// Implementation of <see cref="IMarshalWrapper"/>
    /// </summary>
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
