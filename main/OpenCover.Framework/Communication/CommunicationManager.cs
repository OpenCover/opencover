using System;
using System.IO;
using System.Threading;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Service;

namespace OpenCover.Framework.Communication
{
    /// <summary>
    /// Deals with communcation syncrhonisation
    /// </summary>
    public interface ICommunicationManager
    {
        /// <summary>
        /// Process a communication related message from a profiler
        /// </summary>
        /// <param name="mcb"></param>
        /// <param name="offloadHandling"></param>
        void HandleCommunicationBlock(IManagedCommunicationBlock mcb, Action<ManagedBufferBlock> offloadHandling);

        /// <summary>
        /// process a results block from the profiler
        /// </summary>
        /// <param name="mmb"></param>
        byte[] HandleMemoryBlock(IManagedMemoryBlock mmb);

        /// <summary>
        /// Communication is over
        /// </summary>
        void Complete();
    }

    /// <summary>
    /// Deals with communcation syncrhonisation
    /// </summary>
    public sealed class CommunicationManager : ICommunicationManager
    {
        private readonly IMessageHandler _messageHandler;

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="messageHandler"></param>
        public CommunicationManager(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void HandleCommunicationBlock(IManagedCommunicationBlock mcb, Action<ManagedBufferBlock> offloadHandling)
        {
            mcb.ProfilerRequestsInformation.Reset();

            mcb.StreamAccessorComms.Seek(0, SeekOrigin.Begin);
            mcb.StreamAccessorComms.Read(mcb.DataCommunication, 0, _messageHandler.ReadSize);

            var writeSize = _messageHandler.StandardMessage((MSG_Type)BitConverter.ToInt32(mcb.DataCommunication, 0),
                mcb, SendChunkAndWaitForConfirmation, offloadHandling);

            SendChunkAndWaitForConfirmation(writeSize, mcb);
        }

        private static void SendChunkAndWaitForConfirmation(int writeSize, IManagedCommunicationBlock mcb)
        {
            mcb.StreamAccessorComms.Seek(0, SeekOrigin.Begin);
            mcb.StreamAccessorComms.Write(mcb.DataCommunication, 0, writeSize);

            WaitHandle.SignalAndWait(mcb.InformationReadyForProfiler, mcb.InformationReadByProfiler, 10000, false);
            mcb.InformationReadByProfiler.Reset();
        }

        private byte[] _data = null;
        public byte[] HandleMemoryBlock(IManagedMemoryBlock mmb)
        {
            _data = _data ?? new byte[mmb.BufferSize];
            mmb.ProfilerHasResults.Reset();

            mmb.StreamAccessorResults.Seek(0, SeekOrigin.Begin);
            mmb.StreamAccessorResults.Read(_data, 0, mmb.BufferSize);

            var nCount = (int)BitConverter.ToUInt32(_data, 0);
            var dataSize = (nCount + 1)*sizeof (UInt32);
            var newData = new byte[dataSize];
            Buffer.BlockCopy(_data, 0, newData, 0, dataSize);

            mmb.ResultsHaveBeenReceived.Set();

            return newData;
        }

        public void Complete()
        {
            _messageHandler.Complete();
        }

    }
}
