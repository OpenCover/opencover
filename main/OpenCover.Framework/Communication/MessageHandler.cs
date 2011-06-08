using System;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Service;

namespace OpenCover.Framework.Communication
{
    public interface IMessageHandler
    {
        int StandardMessage(MSG_Type msgType, IntPtr pinnedMemory, IProfilerManager harness);
        int ReadSize { get; }
        int MaxMsgSize { get; }
    }

    public class MessageHandler : IMessageHandler
    {
        const int BufSize = 5;

        private readonly IProfilerCommunication _profilerCommunication;
        private readonly IMarshalWrapper _marshalWrapper;

        public MessageHandler(IProfilerCommunication profilerCommunication, IMarshalWrapper marshalWrapper)
        {
            _profilerCommunication = profilerCommunication;
            _marshalWrapper = marshalWrapper;
        }

        public int StandardMessage(MSG_Type msgType, IntPtr pinnedMemory, IProfilerManager harness)
        {
            var writeSize = 0;
            switch (msgType)
            {
                case MSG_Type.MSG_TrackAssembly:
                    var msgTA = _marshalWrapper.PtrToStructure<MSG_TrackAssembly_Request>(pinnedMemory);
                    var responseTA = new MSG_TrackAssembly_Response();
                    responseTA.track = _profilerCommunication.TrackAssembly(msgTA.module, msgTA.assembly);
                    _marshalWrapper.StructureToPtr(responseTA, pinnedMemory, false);
                    writeSize = Marshal.SizeOf(typeof(MSG_TrackAssembly_Response));
                    break;

                case MSG_Type.MSG_GetSequencePoints:
                    var msgGSP = _marshalWrapper.PtrToStructure<MSG_GetSequencePoints_Request>(pinnedMemory);
                    Service.SequencePoint[] origPoints;
                    var responseCSP = new MSG_GetSequencePoints_Response();
                    _profilerCommunication.GetSequencePoints(msgGSP.module, msgGSP.functionToken, out origPoints);
                    var num = origPoints == null ? 0 : origPoints.Length;

                    var index = 0;
                    var chunk = Marshal.SizeOf(typeof(MSG_SequencePoint));
                    do
                    {
                        writeSize = Marshal.SizeOf(typeof(MSG_GetSequencePoints_Response));
                        responseCSP.more = num > BufSize;
                        responseCSP.count = num > BufSize ? BufSize : num;
                        _marshalWrapper.StructureToPtr(responseCSP, pinnedMemory, false);
                        for (var i = 0; i < responseCSP.count; i++)
                        {
                            var point = new MSG_SequencePoint();
                            point.Offset = origPoints[index].Offset;
                            point.UniqueId = origPoints[index].UniqueId;

                            _marshalWrapper.StructureToPtr(point, pinnedMemory + writeSize, false);
                            writeSize += chunk;
                            index++;
                        }

                        if (responseCSP.more)
                        {
                            harness.SendChunkAndWaitForConfirmation(writeSize);
                            num -= BufSize;
                        }
                    } while (responseCSP.more);

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
                    _readSize = (new[] { Marshal.SizeOf(typeof(MSG_TrackAssembly_Request)), 
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Request)) }).Max();
                }
                return _readSize;
            }
        }

        private int _maxMsgSize;
        public int MaxMsgSize
        {
            get 
            {
                if (_maxMsgSize==0)
                {
                    _maxMsgSize = (new[] {ReadSize, Marshal.SizeOf(typeof (MSG_TrackAssembly_Request)), 
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Response)) + BufSize *  Marshal.SizeOf(typeof(MSG_SequencePoint)) }).Max();
                }
                return _maxMsgSize;
            }
        }
    }

}
