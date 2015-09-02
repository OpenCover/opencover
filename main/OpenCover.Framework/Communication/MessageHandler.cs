//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Model;
using OpenCover.Framework.Service;

namespace OpenCover.Framework.Communication
{
    /// <summary>
    /// Defines the Message Handler
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Process a Standard Message
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="mcb"></param>
        /// <param name="chunkReady"></param>
        /// <param name="offloadHandling"></param>
        /// <returns></returns>
        int StandardMessage(MSG_Type msgType, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady, Action<ManagedBufferBlock> offloadHandling);
        
        /// <summary>
        /// Maximum size of a base message
        /// </summary>
        int ReadSize { get; }
        
        /// <summary>
        /// Finished
        /// </summary>
        void Complete();

    }

    /// <summary>
    /// Implements IMessageHandler
    /// </summary>
    public class MessageHandler : IMessageHandler
    {
        const int GspBufSize = 8000;
        const int GbpBufSize = 2000;

        private readonly IProfilerCommunication _profilerCommunication;
        private readonly IMarshalWrapper _marshalWrapper;
        private readonly IMemoryManager _memoryManager;

        private static readonly ILog DebugLogger = LogManager.GetLogger("DebugLogger");

        /// <summary>
        /// Construct a Message Handler
        /// </summary>
        /// <param name="profilerCommunication"></param>
        /// <param name="marshalWrapper"></param>
        /// <param name="memoryManager"></param>
        public MessageHandler(IProfilerCommunication profilerCommunication, IMarshalWrapper marshalWrapper, IMemoryManager memoryManager)
        {
            _profilerCommunication = profilerCommunication;
            _marshalWrapper = marshalWrapper;
            _memoryManager = memoryManager;
        }

        public int StandardMessage(MSG_Type msgType, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady, Action<ManagedBufferBlock> offloadHandling)
        {
            IntPtr pinnedMemory = mcb.PinnedDataCommunication.AddrOfPinnedObject();
            var writeSize = 0;
            switch (msgType)
            {
                case MSG_Type.MSG_TrackAssembly:
                    writeSize = HandleTrackAssemblyMessage(pinnedMemory);
                    break;

                case MSG_Type.MSG_GetSequencePoints:
                    writeSize = HandleGetSequencePointsMessage(pinnedMemory, mcb, chunkReady);
                    break;

                case MSG_Type.MSG_GetBranchPoints:
                    writeSize = HandleGetBranchPointsMessage(pinnedMemory, mcb, chunkReady);
                    break;

                case MSG_Type.MSG_TrackMethod:
                    writeSize = HandleTrackMethodMessage(pinnedMemory);
                    break;

                case MSG_Type.MSG_AllocateMemoryBuffer:
                    writeSize = HandleAllocateBufferMessage(offloadHandling, pinnedMemory);
                    break;

                case MSG_Type.MSG_CloseChannel:
                    writeSize = HandleCloseChannelMessage(pinnedMemory);
                    break;
            }
            return writeSize;                
        }

        private int HandleGetSequencePointsMessage(IntPtr pinnedMemory, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady)
        {
            var writeSize = 0;
            var response = new MSG_GetSequencePoints_Response();
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_GetSequencePoints_Request>(pinnedMemory);
                InstrumentationPoint[] origPoints;
                _profilerCommunication.GetSequencePoints(request.modulePath, request.assemblyName,
                    request.functionToken, out origPoints);
                var num = origPoints.Maybe(o => o.Length);

                var index = 0;
                var chunk = Marshal.SizeOf(typeof (MSG_SequencePoint));
                do
                {
                    writeSize = Marshal.SizeOf(typeof (MSG_GetSequencePoints_Response));
                    response.more = num > GspBufSize;
                    response.count = num > GspBufSize ? GspBufSize : num;
                    _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
                    var point = new MSG_SequencePoint();
                    for (var i = 0; i < response.count; i++)
                    {
                        point.offset = origPoints[index].Offset;
                        point.uniqueId = origPoints[index].UniqueSequencePoint;

                        _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                        writeSize += chunk;
                        index++;
                    }

                    if (response.more)
                    {
                        chunkReady(writeSize, mcb);
                        num -= GspBufSize;
                    }
                } while (response.more);
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandleGetSequencePointsMessage => {0}:{1}", ex.GetType(), ex.Message);
                response.more = false;
                response.count = 0;
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
            }
            return writeSize;
        }

        private int HandleGetBranchPointsMessage(IntPtr pinnedMemory, IManagedCommunicationBlock mcb, Action<int, IManagedCommunicationBlock> chunkReady)
        {
            var writeSize = 0;
            var response = new MSG_GetBranchPoints_Response();
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_GetBranchPoints_Request>(pinnedMemory);
                BranchPoint[] origPoints;
                _profilerCommunication.GetBranchPoints(request.modulePath, request.assemblyName,
                    request.functionToken, out origPoints);
                var num = origPoints.Maybe(o => o.Length);

                var index = 0;
                var chunk = Marshal.SizeOf(typeof (MSG_BranchPoint));
                do
                {
                    writeSize = Marshal.SizeOf(typeof (MSG_GetBranchPoints_Response));
                    response.more = num > GbpBufSize;
                    response.count = num > GbpBufSize ? GbpBufSize : num;
                    _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
                    var point = new MSG_BranchPoint();
                    for (var i = 0; i < response.count; i++)
                    {
                        point.offset = origPoints[index].Offset;
                        point.uniqueId = origPoints[index].UniqueSequencePoint;
                        point.path = origPoints[index].Path;

                        _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                        writeSize += chunk;
                        index++;
                    }

                    if (response.more)
                    {
                        chunkReady(writeSize, mcb);
                        num -= GbpBufSize;
                    }
                } while (response.more);
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandleGetBranchPointsMessage => {0}:{1}", ex.GetType(), ex.Message);
                response.more = false;
                response.count = 0;
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
            }
            return writeSize;
        }

        private int HandleAllocateBufferMessage(Action<ManagedBufferBlock> offloadHandling, IntPtr pinnedMemory)
        {
            var writeSize = Marshal.SizeOf(typeof(MSG_AllocateBuffer_Response));
            var response = new MSG_AllocateBuffer_Response { allocated = false, bufferId = 0 };
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_AllocateBuffer_Request>(pinnedMemory);
                uint bufferId;
                var block = _memoryManager.AllocateMemoryBuffer(request.bufferSize, out bufferId);
                response.allocated=true;
                response.bufferId = bufferId;
                offloadHandling(block);
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandlerAllocateBufferMessage => {0}:{1}", ex.GetType(), ex.Message);
                response.allocated = false;
                response.bufferId = 0;
            }
            finally
            {
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
            }
            return writeSize;
        }

        private int HandleCloseChannelMessage(IntPtr pinnedMemory)
        {
            var writeSize = Marshal.SizeOf(typeof(MSG_CloseChannel_Response));
            var response = new MSG_CloseChannel_Response { done = true };
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_CloseChannel_Request>(pinnedMemory);
                var bufferId = request.bufferId;
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
                _memoryManager.DeactivateMemoryBuffer(bufferId);
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandleCloseChannelMessage => {0}:{1}", ex.GetType(), ex.Message);
            }
            finally
            {
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
            }
            return writeSize;
        }

        private int HandleTrackMethodMessage(IntPtr pinnedMemory)
        {
            var writeSize = Marshal.SizeOf(typeof(MSG_TrackMethod_Response));
            var response = new MSG_TrackMethod_Response();
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_TrackMethod_Request>(pinnedMemory);
                uint uniqueId;
                response.track = _profilerCommunication.TrackMethod(request.modulePath,
                    request.assemblyName, request.functionToken, out uniqueId);
                response.uniqueId = uniqueId;
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandleTrackMethodMessage => {0}:{1}", ex.GetType(), ex.Message);
                response.track = false;
                response.uniqueId = 0;
            }
            finally
            {
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
            }
            return writeSize;
        }

        private int HandleTrackAssemblyMessage(IntPtr pinnedMemory)
        {
            var response = new MSG_TrackAssembly_Response();
            var writeSize = Marshal.SizeOf(typeof(MSG_TrackAssembly_Response));
            try
            {
                var request = _marshalWrapper.PtrToStructure<MSG_TrackAssembly_Request>(pinnedMemory);
                response.track = _profilerCommunication.TrackAssembly(request.modulePath, request.assemblyName);
            }
            catch (Exception ex)
            {
                DebugLogger.ErrorFormat("HandleTrackAssemblyMessage => {0}:{1}", ex.GetType(), ex.Message);
                response.track = false;
            }
            finally
            {
                _marshalWrapper.StructureToPtr(response, pinnedMemory, false);
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
                        Marshal.SizeOf(typeof(MSG_CloseChannel_Response)) 
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
