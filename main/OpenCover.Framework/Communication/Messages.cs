//
// OpenCover - S. Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Runtime.InteropServices;

namespace OpenCover.Framework.Communication
{
    /// <summary>
    /// The command supportd by the host
    /// </summary>
// ReSharper disable InconsistentNaming
    public enum MSG_Type : int
    {
        /// <summary>
        /// Does this assembly have any code that can be covered
        /// </summary>
        MSG_TrackAssembly = 1,

        /// <summary>
        /// Get the sequence point for a method
        /// </summary>
        MSG_GetSequencePoints = 2,

        /// <summary>
        /// Get the branch points for a method
        /// </summary>
        MSG_GetBranchPoints = 3,

        /// <summary>
        /// Do we track this method (test methods only)
        /// </summary>
        MSG_TrackMethod = 4,

        /// <summary>
        /// allocate a provate memory buffer for profiler/host communications
        /// </summary>
        MSG_AllocateMemoryBuffer = 5,

        /// <summary>
        /// Close a channel between host and profiler
        /// </summary>
        MSG_CloseChannel = 6,

        /// <summary>
        /// Do we track this process
        /// </summary>
        MSG_TrackProcess = 7,
    }

    /// <summary>
    /// The type of results
    /// </summary>
    public enum MSG_IdType : uint
    {
        /// <summary>
        /// a basic coverage vist point
        /// </summary>
        IT_VisitPoint = 0x00000000,

        /// <summary>
        /// A test method enter
        /// </summary>
        IT_MethodEnter = 0x40000000,

        /// <summary>
        /// A test method exit
        /// </summary>
        IT_MethodLeave = 0x80000000,

        /// <summary>
        /// A test method tail call
        /// </summary>
        IT_MethodTailcall = 0xC0000000,

        /// <summary>
        /// A mask of the above
        /// </summary>
        IT_Mask = 0x3FFFFFFF,
    }

    /// <summary>
    /// Track an assembly
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_TrackAssembly_Request
    {
        /// <summary>
        /// The message type
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The path to the process
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string processName;

        /// <summary>
        /// The path to the module/assembly
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;

        /// <summary>
        /// The name of the module/assembly
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    /// <summary>
    /// The response to a <see cref="MSG_TrackAssembly_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_TrackAssembly_Response
    {
        /// <summary>
        /// True - if the assembly has instrumentable code
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool track;
    }

    /// <summary>
    /// Get the sequence points for a method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_GetSequencePoints_Request
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The token of the method
        /// </summary>
        public int functionToken;

        /// <summary>
        /// The path to the process
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string processName;

        /// <summary>
        /// The path to the module hosting the emthod
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;

        /// <summary>
        /// The name of the module/assembly hosting the method
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    /// <summary>
    /// Defines a seqence point
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_SequencePoint
    {
        /// <summary>
        /// The identifier of the sequence point
        /// </summary>
        public uint uniqueId;

        /// <summary>
        /// The original IL offset of where the sequence pont should be placed
        /// </summary>
        public int offset;
    }

    /// <summary>
    /// The response to a <see cref="MSG_GetSequencePoints_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_GetSequencePoints_Response
    {
        /// <summary>
        /// Do we have more data
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool more;

        /// <summary>
        /// The number of sequence points that follow
        /// </summary>
        public int count;
    }

    /// <summary>
    /// Get the branch points of a method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_GetBranchPoints_Request
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The token of the method
        /// </summary>
        public int functionToken;

        /// <summary>
        /// The path to the process
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string processName;

        /// <summary>
        /// The path to the module hosting the emthod
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;

        /// <summary>
        /// The name of the module/assembly hosting the method
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    /// <summary>
    /// Defines a branch point
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_BranchPoint
    {
        /// <summary>
        /// The uniqueid of the branch point
        /// </summary>
        public uint uniqueId;

        /// <summary>
        /// The original IL offset of the branch instruction to be instrumented
        /// </summary>
        public int offset;

        /// <summary>
        /// Which of the paths T/F or switch that the point is intending to cover.
        /// </summary>
        public int path;
    }

    /// <summary>
    /// The response to a <see cref="MSG_GetBranchPoints_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_GetBranchPoints_Response
    {
        /// <summary>
        /// Do we have more data
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool more;

        /// <summary>
        /// The number of branch points that follow
        /// </summary>
        public int count;
    }

    /// <summary>
    /// Should we track this method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_TrackMethod_Request
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The token of the method under test
        /// </summary>
        public int functionToken;

        /// <summary>
        /// Te path to the module
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;

        /// <summary>
        /// The name of the assemby/module
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    /// <summary>
    /// the response to a <see cref="MSG_TrackMethod_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_TrackMethod_Response
    {
        /// <summary>
        /// True - the method should be tracked
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool track;

        /// <summary>
        /// The uniqueid assigned to the method
        /// </summary>
        public uint uniqueId;
    }

    /// <summary>
    /// Request to allocate a buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_AllocateBuffer_Request
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The buffer size
        /// </summary>
        public int bufferSize;
    }

    /// <summary>
    /// The response to a <see cref="MSG_AllocateBuffer_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_AllocateBuffer_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool allocated;
        public uint bufferId;
    }

    /// <summary>
    /// A close channel request
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_CloseChannel_Request
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The id of the buffer to close
        /// </summary>
        public uint bufferId;
    }

    /// <summary>
    /// The response to a <see cref="MSG_CloseChannel_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_CloseChannel_Response
    {
        /// <summary>
        /// The buffer should be cleared.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool done;
    }

    /// <summary>
    /// Track an assembly
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_TrackProcess_Request
    {
        /// <summary>
        /// The message type
        /// </summary>
        public MSG_Type type;

        /// <summary>
        /// The path to the process
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string processName;
    }

    /// <summary>
    /// The response to a <see cref="MSG_TrackProcess_Request"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_TrackProcess_Response
    {
        /// <summary>
        /// True - if the assembly has instrumentable code
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool track;
    }
    // ReSharper restore InconsistentNaming

}
