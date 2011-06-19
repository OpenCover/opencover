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
        private const string UserRegistrationString = "/n /i:user";

        /// <summary>
        /// Register the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        /// <param name="is64">true - register 64 bit</param>
        public static void Register(bool userRegistration, bool is64)
        {
            ExecuteRegsvr32(userRegistration, is64, true);
        }

        /// <summary>
        /// Unregister the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        /// <param name="is64">true - unregister 64 bit</param>
        public static void Unregister(bool userRegistration, bool is64)
        {
            ExecuteRegsvr32(userRegistration, is64, false);
        }

        private static void ExecuteRegsvr32(bool userRegistration, bool is64, bool register)
        {
            var startInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "regsvr32.exe"),
                string.Format("/s {2} {0} \"{1}\"", userRegistration ? UserRegistrationString : String.Empty,
                GetProfilerPath(is64), register ? string.Empty : "/u")) { CreateNoWindow = true };

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyLocation()
        {
            return Path.GetDirectoryName(typeof(ProfilerRegistration).Assembly.Location ?? string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetProfilerPath(bool is64)
        {
            return Path.Combine(GetAssemblyLocation(), is64 ? "x64" : "x86") + @"\OpenCover.Profiler.dll";
        }
    }
}
