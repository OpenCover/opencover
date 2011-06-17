//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenCover.Framework
{
    /// <summary>
    /// Used to register and unregister the profiler 
    /// </summary>
    /// <remarks>
    /// Intentionally not unit tested - as this is calling regsvr32 which does what it does and does not need more testing from me
    /// </remarks>
    public class ProfilerRegistration
    {
        private const string UserRegistrationString = "/n /i:user ";
        /// <summary>
        /// Register the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        public static void Register(bool userRegistration)
        {
            var startInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "regsvr32.exe"), 
                string.Format("/s {0}OpenCover.Profiler.dll", userRegistration ? UserRegistrationString : String.Empty)) { CreateNoWindow = true };

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        /// <summary>
        /// Unregister the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        public static void Unregister(bool userRegistration)
        {
            var startInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "regsvr32.exe"),
                string.Format("/s /u {0}OpenCover.Profiler.dll", userRegistration ? UserRegistrationString : String.Empty)) { CreateNoWindow = true };

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}
