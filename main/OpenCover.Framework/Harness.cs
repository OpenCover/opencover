using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using OpenCover.Framework.Service;

namespace OpenCover.Framework
{
    public class Harness
    {
        public enum MSG_Type : int
        {
            MSG_TrackAssembly = 1,
            MSG_GetSequencePoints = 2,
        }

        [StructLayout(LayoutKind.Sequential, Pack=1, CharSet = CharSet.Unicode)]
        public struct MSG_TrackAssembly_Request1
        {
            public MSG_Type type;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string module;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string assembly;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_TrackAssembly_Response
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool track;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        public struct MSG_GetSequencePoints_Request
        {
            public MSG_Type type;
            public int functionToken;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string module;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SequencePoint
        {
            public uint UniqueId;
            public int Offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_GetSequencePoints_Response
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool more;

            public int count;

            //[MarshalAs(UnmanagedType.LPArray,  = typeof(SequencePoint))]
            //public SequencePoint[] points;  
        }

        public static void RunProcess(Action<Action<StringDictionary>> process, IProfilerCommunication communication)
        {
            var key = Guid.NewGuid().GetHashCode().ToString("X");
            var processClosed = new AutoResetEvent(false);
            var handles = new List<WaitHandle> {processClosed};

            var requestDataReady = new EventWaitHandle(false, EventResetMode.ManualReset, "Local\\OpenCover_Profiler_Communication_SendData_Event_" + key);
            var responseDataReady = new EventWaitHandle(false, EventResetMode.ManualReset, "Local\\OpenCover_Profiler_Communication_ReceiveData_Event_" + key);

            handles.Add(requestDataReady);
            const int msgSize = 4096;

            using (var mmf = MemoryMappedFile.CreateNew("OpenCover_Profiler_Communication_MemoryMapFile_" + key, msgSize))
            using (var streamAccessor = mmf.CreateViewStream(0, msgSize, MemoryMappedFileAccess.ReadWrite))
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        process(dictionary =>
                        {
                            if (dictionary == null) return;
                            dictionary.Add("OpenCover_Profiler_Key", key);
                        });
                    }
                    finally
                    {
                        processClosed.Set();
                    }
                });

                const int bufSize = 400;

                var continueWait = true;
                do
                {
                    
                    switch (WaitHandle.WaitAny(handles.ToArray()))
                    {
                        case 1:
                            
                            var data = new byte[4096];
                            streamAccessor.Seek(0, SeekOrigin.Begin);
                            streamAccessor.Read(data, 0, 4096);

                            var msgType = (MSG_Type)BitConverter.ToInt32(data, 0);
                            var pinned = GCHandle.Alloc(data, GCHandleType.Pinned);

                            switch(msgType)
                            {
                                case MSG_Type.MSG_TrackAssembly:
                                    var msgTA = (MSG_TrackAssembly_Request1)Marshal.PtrToStructure(pinned.AddrOfPinnedObject(), typeof (MSG_TrackAssembly_Request1));
                                    var responseTA = new MSG_TrackAssembly_Response();
                                    responseTA.track = communication.TrackAssembly(msgTA.module, msgTA.assembly);
                                    Marshal.StructureToPtr(responseTA, pinned.AddrOfPinnedObject(), false);
                                    break;

                                case MSG_Type.MSG_GetSequencePoints:
                                    var msgGSP = (MSG_GetSequencePoints_Request)Marshal.PtrToStructure(pinned.AddrOfPinnedObject(), typeof (MSG_GetSequencePoints_Request));
                                    Service.SequencePoint[] origPoints;
                                    var responseCSP = new MSG_GetSequencePoints_Response();
                                    communication.GetSequencePoints(msgGSP.module, msgGSP.functionToken, out origPoints);
                                    var num = origPoints == null ? 0 : origPoints.Length;
                                    
                                    var index = 0;
                                    do
                                    {
                                        responseCSP.more = num > bufSize;
                                        responseCSP.count = num > bufSize ? bufSize : num;
                                        Marshal.StructureToPtr(responseCSP, pinned.AddrOfPinnedObject(), false);
                                        for (var i = 0; i < responseCSP.count; i++)
                                        {
                                            var point = new SequencePoint()
                                                            {
                                                                Offset = origPoints[index].Offset,
                                                                UniqueId = origPoints[index].UniqueId
                                                            };
                                            Marshal.StructureToPtr(point, pinned.AddrOfPinnedObject() + 8 + (i * 8), false);
                                            index++;
                                        }
                                        
                                        if (responseCSP.more)
                                        {
                                            pinned.Free();
                                            streamAccessor.Seek(0, SeekOrigin.Begin);
                                            streamAccessor.Write(data, 0, 4096);
                                            requestDataReady.Reset();
                                            responseDataReady.Set();
                                            responseDataReady.Reset();

                                            WaitHandle.WaitAny(new[] {requestDataReady});
                                            pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
                                            num -= bufSize;
                                        }
                                    } while (responseCSP.more);
                                    
                                    break;
                                default:
                                    break;
                            }
                            
                            pinned.Free();
                            streamAccessor.Seek(0, SeekOrigin.Begin);
                            streamAccessor.Write(data, 0, 4096);
                            requestDataReady.Reset();
                            responseDataReady.Set();
                            responseDataReady.Reset();
                            break;

                        default:
                            continueWait = false;
                            break;
                    }
                } while (continueWait);
            }

        }
    }
}
