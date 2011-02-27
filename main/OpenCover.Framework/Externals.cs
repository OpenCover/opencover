using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenCover.Framework
{
    public class Externals
    {
        [DllImport("OpenCover.Profiler.dll")]
        public static extern uint DllInstall(int bInstall, [MarshalAs(UnmanagedType.LPWStr)] string pszCmdLine);
    }

    
}
