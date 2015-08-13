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
        int StandardMessage(MSG_Type msgType, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady, Action<ManagedBufferBlock> offloadHandling);
        int ReadSize { get; }
        void Complete();
    }

    public class MessageHandler : IMessageHandler
    {
        const int GSP_BufSize = 50;
        const int GBP_BufSize = 50;

        private readonly IProfilerCommunication _profilerCommunication;
        private readonly IMarshalWrapper _marshalWrapper;
        private readonly IMemoryManager _memoryManager;

        public MessageHandler(IProfilerCommunication profilerCommunication, IMarshalWrapper marshalWrapper, IMemoryManager memoryManager)
        {
            _profilerCommunication = profilerCommunication;
            _marshalWrapper = marshalWrapper;
            _memoryManager = memoryManager;
        }

        // TODO: change pinnedMemory to an byte[], pass in mcb as well
        public int StandardMessage(MSG_Type msgType, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady, Action<ManagedBufferBlock> offloadHandling)
        {
            IntPtr pinnedMemory = mcb.PinnedDataCommunication.AddrOfPinnedObject();
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
                        var responseGSP = new MSG_GetSequencePoints_Response();
                        try
                        {
                            var msgGSP = _marshalWrapper.PtrToStructure<MSG_GetSequencePoints_Request>(pinnedMemory);
                            InstrumentationPoint[] origPoints;
                            _profilerCommunication.GetSequencePoints(msgGSP.modulePath, msgGSP.assemblyName,
                                msgGSP.functionToken, out origPoints);
                            var num = origPoints == null ? 0 : origPoints.Length;

                            var index = 0;
                            var chunk = Marshal.SizeOf(typeof (MSG_SequencePoint));
                            do
                            {
                                writeSize = Marshal.SizeOf(typeof (MSG_GetSequencePoints_Response));
                                responseGSP.more = num > GSP_BufSize;
                                responseGSP.count = num > GSP_BufSize ? GSP_BufSize : num;
                                _marshalWrapper.StructureToPtr(responseGSP, pinnedMemory, false);
                                for (var i = 0; i < responseGSP.count; i++)
                                {
                                    var point = new MSG_SequencePoint();
                                    point.offset = origPoints[index].Offset;
                                    point.uniqueId = origPoints[index].UniqueSequencePoint;

                                    _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                                    writeSize += chunk;
                                    index++;
                                }

                                if (responseGSP.more)
                                {
                                    chunkReady(writeSize, mcb);
                                    num -= GSP_BufSize;
                                }
                            } while (responseGSP.more);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}:{1}", ex.GetType(), ex.Message);
                            responseGSP.more = false;
                            responseGSP.count = 0;
                            _marshalWrapper.StructureToPtr(responseGSP, pinnedMemory, false);
                        }
                    }
                    break;

                case MSG_Type.MSG_GetBranchPoints:
                    {
                        var responseGBP = new MSG_GetBranchPoints_Response();
                        try
                        {
                            var msgGBP = _marshalWrapper.PtrToStructure<MSG_GetBranchPoints_Request>(pinnedMemory);
                            BranchPoint[] origPoints;
                            _profilerCommunication.GetBranchPoints(msgGBP.modulePath, msgGBP.assemblyName,
                                msgGBP.functionToken, out origPoints);
                            var num = origPoints == null ? 0 : origPoints.Length;

                            var index = 0;
                            var chunk = Marshal.SizeOf(typeof (MSG_BranchPoint));
                            do
                            {
                                writeSize = Marshal.SizeOf(typeof (MSG_GetBranchPoints_Response));
                                responseGBP.more = num > GBP_BufSize;
                                responseGBP.count = num > GBP_BufSize ? GBP_BufSize : num;
                                _marshalWrapper.StructureToPtr(responseGBP, pinnedMemory, false);
                                for (var i = 0; i < responseGBP.count; i++)
                                {
                                    var point = new MSG_BranchPoint();
                                    point.offset = origPoints[index].Offset;
                                    point.uniqueId = origPoints[index].UniqueSequencePoint;
                                    point.path = origPoints[index].Path;

                                    _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                                    writeSize += chunk;
                                    index++;
                                }

                                if (responseGBP.more)
                                {
                                    chunkReady(writeSize, mcb);
                                    num -= GBP_BufSize;
                                }
                            } while (responseGBP.more);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}:{1}", ex.GetType(), ex.Message);
                            responseGBP.more = false;
                            responseGBP.count = 0;
                            _marshalWrapper.StructureToPtr(responseGBP, pinnedMemory, false);
                        }
                    }
                    break;

                case MSG_Type.MSG_TrackMethod:
                    {
                        var msgTM = _marshalWrapper.PtrToStructure<MSG_TrackMethod_Request>(pinnedMemory);
                        var responseTM = new MSG_TrackMethod_Response();
                        uint uniqueId;
                        responseTM.track = _profilerCommunication.TrackMethod(msgTM.modulePath, 
                            msgTM.assemblyName, msgTM.functionToken, out uniqueId);
                        responseTM.uniqueId = uniqueId;
                        _marshalWrapper.StructureToPtr(responseTM, pinnedMemory, false);
                        writeSize = Marshal.SizeOf(typeof(MSG_TrackMethod_Response));
                    }
                    break;

                case MSG_Type.MSG_AllocateMemoryBuffer:
                    {
                        var msgAB = _marshalWrapper.PtrToStructure<MSG_AllocateBuffer_Request>(pinnedMemory);
                        
                        var block = _memoryManager.AllocateMemoryBuffer(msgAB.bufferSize, _bufferId);

                        var responseAB = new MSG_AllocateBuffer_Response {allocated = true, bufferId = _bufferId };
                        _marshalWrapper.StructureToPtr(responseAB, pinnedMemory, false);
                        writeSize = Marshal.SizeOf(typeof(MSG_AllocateBuffer_Response));
                        _bufferId++;

                        offloadHandling(block);
                    }
                    break;

                case MSG_Type.MSG_CloseChannel:
                    {
                        var msgCC = _marshalWrapper.PtrToStructure<MSG_CloseChannel_Request>(pinnedMemory);
                        var bufferId = msgCC.bufferId;

                        var responseCC = new MSG_CloseChannel_Response { done = true };
                        _marshalWrapper.StructureToPtr(responseCC, pinnedMemory, false);
                        writeSize = Marshal.SizeOf(typeof(MSG_CloseChannel_Response));

                        _memoryManager.DeactivateMemoryBuffer(bufferId);
                    }
                    break;
            }
            return writeSize;                
        }

        private int _readSize;
        private uint _bufferId = 0;

        public int ReadSize
        {
            get
            {
                if (_readSize == 0)
                {
                    _readSize = (new[] { 
                        Marshal.SizeOf(typeof(MSG_TrackAssembly_Request)), 
                        Marshal.SizeOf(typeof(MSG_TrackAssembly_Response)), 
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Request)),
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Response)),
                        Marshal.SizeOf(typeof(MSG_GetBranchPoints_Request)),
                        Marshal.SizeOf(typeof(MSG_GetBranchPoints_Response)),
                        Marshal.SizeOf(typeof(MSG_TrackMethod_Request)), 
                        Marshal.SizeOf(typeof(MSG_TrackMethod_Response)), 
                        Marshal.SizeOf(typeof(MSG_AllocateBuffer_Request)), 
                        Marshal.SizeOf(typeof(MSG_AllocateBuffer_Response)), 
                        Marshal.SizeOf(typeof(MSG_CloseChannel_Request)), 
                        Marshal.SizeOf(typeof(MSG_CloseChannel_Response)), 
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
