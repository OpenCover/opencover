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
            MSG_TrackAssembly = 1    
        }

        private static int Incr(ref int index, int increment)
        {
            var ret = index;
            index += increment;
            return ret;
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
            using (var msgAccessor = mmf.CreateViewAccessor(0, msgSize, MemoryMappedFileAccess.ReadWrite))
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

                var continueWait = true;
                do
                {
                    streamAccessor.Seek(0, SeekOrigin.Begin);
                    int index = 0;
                    switch (WaitHandle.WaitAny(handles.ToArray()))
                    {
                        case 1:
                            MSG_Type u;
                            msgAccessor.Read(Incr(ref index, 4), out u);

                            Debug.WriteLine("msg => {0}", u);

                            switch(u)
                            {
                                case MSG_Type.MSG_TrackAssembly:
                                    var nModule = msgAccessor.ReadInt16(Incr(ref index, 2));
                                    
                                    var module = new byte[1024];
                                    streamAccessor.Seek(Incr(ref index, 1024), SeekOrigin.Begin);
                                    streamAccessor.Read(module, 0, 1024);
                                    var moduleName = Encoding.Unicode.GetString(module).Substring(0, nModule).Trim();

                                    var nAssembly = msgAccessor.ReadInt16(Incr(ref index, 2));
                                    streamAccessor.Seek(Incr(ref index, 1024), SeekOrigin.Begin);
                                    streamAccessor.Read(module, 0, 1024);
                                    var assemblyName = Encoding.Unicode.GetString(module).Substring(0, nAssembly).Trim();

                                    Debug.WriteLine("=> {1} => {0}", moduleName, moduleName.Length);
                                    Debug.WriteLine("=> {1} => {0}", assemblyName, assemblyName.Length);

                                    var response = communication.TrackAssembly(moduleName, assemblyName);
                                    Debug.WriteLine("TA => => {0}", response);


                                    msgAccessor.Write(0, response ? 1 : 0);

                                    break;
                                default:
                                    break;
                            }

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
