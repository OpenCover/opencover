using System;
using System.Collections.Specialized;

namespace OpenCover.Framework.Manager
{
    public interface IProfilerManager
    {
        void RunProcess(Action<Action<StringDictionary>> process);
        void SendChunkAndWaitForConfirmation(int writeSize);
    }
}