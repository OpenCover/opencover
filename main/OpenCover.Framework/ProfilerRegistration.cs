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
using System.Threading;

namespace OpenCover.Framework
{
    /// <summary>
    /// Used to register and unregister the profiler 
    /// </summary>
    /// <remarks>
    /// Intentionally not unit tested - as this is calling regsvr32 which does what it does and does not need more testing from me
    /// </remarks>
    [ExcludeFromCoverage("Intentionally not unit tested - as this is calling regsvr32 which does what it does and does not need more testing from me")]
    public class ProfilerRegistration
    {
        private const string UserRegistrationString = "/n /i:user";

        /// <summary>
        /// Register the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        public static void Register(bool userRegistration)
        {
            ExecuteRegsvr32(userRegistration, true);
        }

        /// <summary>
        /// Unregister the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="userRegistration">true - user the /n /i:user switches</param>
        public static void Unregister(bool userRegistration)
        {
            ExecuteRegsvr32(userRegistration, false);
        }

        private static void ExecuteRegsvr32(bool userRegistration, bool register)
        {
            ExecuteRegsvr32(userRegistration, register, false);
            if (Environment.Is64BitOperatingSystem) { ExecuteRegsvr32(userRegistration, register, true); }
        }

        private static void ExecuteRegsvr32(bool userRegistration, bool register, bool is64)
        {
            var startInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "regsvr32.exe"),
                                     string.Format("/s {2} {0} \"{1}\"",userRegistration ? UserRegistrationString : String.Empty,
                                     GetProfilerPath(is64), register ? string.Empty : "/u")) {CreateNoWindow = true};

            var process = Process.Start(startInfo);
            process.WaitForExit();
            if (register && 0 != process.ExitCode) // there is an oddity where unregistering the x64 version after the x86 (or vice versa) issues an access denied (5)
            {
                throw new InvalidOperationException(
                    string.Format("Failed to register(user:{0},register:{1},is64:{2}):{3} the profiler assembly; you may want to look into permissions or using the -register:user option instead. {4} {5}",
                        userRegistration, register, is64, process.ExitCode, process.StartInfo.FileName, process.StartInfo.Arguments));
            }
        }

        /// <summary>
        /// Get the current location of this assembly
        /// </summary>
        /// <returns></returns>
        private static string GetAssemblyLocation()
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
