//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Service;
using SequencePoint = OpenCover.Framework.Model.SequencePoint;

namespace OpenCover.Framework.Communication
{
    public interface IMessageHandler
    {
        int StandardMessage(MSG_Type msgType, IntPtr pinnedMemory, Action<int> chunkReady);
        int ReadSize { get; }

        void Complete();
    }

    public class MessageHandler : IMessageHandler
    {
        const int GSP_BufSize = 50;
        const int GBP_BufSize = 50;

        private readonly IProfilerCommunication _profilerCommunication;
        private readonly IMarshalWrapper _marshalWrapper;

        public MessageHandler(IProfilerCommunication profilerCommunication, IMarshalWrapper marshalWrapper)
        {
            _profilerCommunication = profilerCommunication;
            _marshalWrapper = marshalWrapper;
        }

        public int StandardMessage(MSG_Type msgType, IntPtr pinnedMemory, Action<int> chunkReady)
        {
            var writeSize = 0;
            switch (msgType)
            {
                case MSG_Type.MSG_TrackAssembly:
                    {
                        var msgTA = _marshalWrapper.PtrToStructure<MSG_TrackAssembly_Request>(pinnedMemory);
                        var responseTA = new MSG_TrackAssembly_Response();
                        responseTA.track = _profilerCommunication.TrackAssembly(msgTA.modulePath, msgTA.assemblyName);
                        _marshalWrapper.StructureToPtr(responseTA, pinnedMemory, false);
                        writeSize = Marshal.SizeOf(typeof (MSG_TrackAssembly_Response));
                    }
                    break;

                case MSG_Type.MSG_GetSequencePoints:
                    {
                        var msgGSP = _marshalWrapper.PtrToStructure<MSG_GetSequencePoints_Request>(pinnedMemory);
                        InstrumentationPoint[] origPoints;
                        var responseCSP = new MSG_GetSequencePoints_Response();
                        _profilerCommunication.GetSequencePoints(msgGSP.modulePath, msgGSP.assemblyName,
                                                                 msgGSP.functionToken, out origPoints);
                        var num = origPoints == null ? 0 : origPoints.Length;

                        var index = 0;
                        var chunk = Marshal.SizeOf(typeof (MSG_SequencePoint));
                        do
                        {
                            writeSize = Marshal.SizeOf(typeof (MSG_GetSequencePoints_Response));
                            responseCSP.more = num > GSP_BufSize;
                            responseCSP.count = num > GSP_BufSize ? GSP_BufSize : num;
                            _marshalWrapper.StructureToPtr(responseCSP, pinnedMemory, false);
                            for (var i = 0; i < responseCSP.count; i++)
                            {
                                var point = new MSG_SequencePoint();
                                point.Offset = origPoints[index].Offset;
                                point.UniqueId = origPoints[index].UniqueSequencePoint;

                                _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                                writeSize += chunk;
                                index++;
                            }

                            if (responseCSP.more)
                            {
                                chunkReady(writeSize);
                                num -= GSP_BufSize;
                            }
                        } while (responseCSP.more);
                    }
                    break;

                case MSG_Type.MSG_GetBranchPoints:
                    {
                        var msgGBP = _marshalWrapper.PtrToStructure<MSG_GetBranchPoints_Request>(pinnedMemory);
                        BranchPoint[] origPoints;
                        var responseCSP = new MSG_GetBranchPoints_Response();
                        _profilerCommunication.GetBranchPoints(msgGBP.modulePath, msgGBP.assemblyName,
                                                                 msgGBP.functionToken, out origPoints);
                        var num = origPoints == null ? 0 : origPoints.Length;
           
                        var index = 0;
                        var chunk = Marshal.SizeOf(typeof (MSG_BranchPoint));
                        do
                        {
                            writeSize = Marshal.SizeOf(typeof (MSG_GetBranchPoints_Response));
                            responseCSP.more = num > GBP_BufSize;
                            responseCSP.count = num > GBP_BufSize ? GBP_BufSize : num;
                            _marshalWrapper.StructureToPtr(responseCSP, pinnedMemory, false);
                            for (var i = 0; i < responseCSP.count; i++)
                            {
                                var point = new MSG_BranchPoint();
                                point.Offset = origPoints[index].Offset;
                                point.UniqueId = origPoints[index].UniqueSequencePoint;
                                point.Path = origPoints[index].Path;

                                _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                                writeSize += chunk;
                                index++;
                            }

                            if (responseCSP.more)
                            {
                                chunkReady(writeSize);
                                num -= GBP_BufSize;
                            }
                        } while (responseCSP.more);
                    }
                    break;

                case MSG_Type.MSG_TrackMethod:
                    {
                        var msgTM = _marshalWrapper.PtrToStructure<MSG_TrackMethod_Request>(pinnedMemory);
                        var responseTM = new MSG_TrackMethod_Response();
                        uint uniqueId;
                        responseTM.track = _profilerCommunication.TrackMethod(msgTM.modulePath, 
                            msgTM.assemblyName, msgTM.functionToken, out uniqueId);
                        responseTM.UniqueId = uniqueId;
                        _marshalWrapper.StructureToPtr(responseTM, pinnedMemory, false);
                        writeSize = Marshal.SizeOf(typeof(MSG_TrackMethod_Response));
                    }
                    break;
            }
            return writeSize;                
        }

        private int _readSize;
        public int ReadSize
        {
            get
            {
                if (_readSize == 0)
                {
                    _readSize = (new[] { 
                        Marshal.SizeOf(typeof(MSG_TrackAssembly_Request)), 
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Request)),
                        Marshal.SizeOf(typeof(MSG_GetBranchPoints_Request)),
                        Marshal.SizeOf(typeof(MSG_TrackMethod_Request)), 
                    }).Max();
                }
                return _readSize;
            }
        }

        public void Complete()
        {
            _profilerCommunication.Stopping();
        }
    }

}
