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
using log4net;

namespace OpenCover.Framework
{
    /// <summary>
    /// This exception is thrown in special cases where program termination must stop 
    /// but there is no need to raise a crash report
    /// </summary>
    public class ExitApplicationWithoutReportingException : Exception
    {
        
    }

    /// <summary>
    /// Used to register and unregister the profiler 
    /// </summary>
    /// <remarks>
    /// Intentionally not unit tested - as this is calling regsvr32 which does what it does and does not need more testing from me
    /// </remarks>
    [ExcludeFromCoverage("Intentionally not unit tested - as this is calling regsvr32 which does what it does and does not need more testing from me")]
    public static class ProfilerRegistration
    {
        private const string UserRegistrationString = "/n /i:user";

        private static readonly ILog Logger = LogManager.GetLogger("OpenCover");

        /// <summary>
        /// Register the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="registration">User - use the /n /i:user switches</param>
        public static void Register(Registration registration)
        {
            if (registration == Registration.Normal || registration == Registration.User)
            {
                ExecuteRegsvr32(registration == Registration.User, true);                
            }
        }

        /// <summary>
        /// Unregister the profiler using %SystemRoot%\system\regsvr32.exe
        /// </summary>
        /// <param name="registration">User - use the /n /i:user switches</param>
        public static void Unregister(Registration registration)
        {
            if (registration == Registration.Normal || registration == Registration.User)
            {
                ExecuteRegsvr32(registration == Registration.User, false);
            }
        }

        private static void ExecuteRegsvr32(bool userRegistration, bool register)
        {
            ExecuteRegsvr32(userRegistration, register, false);
            if (Environment.Is64BitOperatingSystem)
            {
                ExecuteRegsvr32(userRegistration, register, true);
            }
        }

        private static int ExecuteProcess(ProcessStartInfo psi)
        {
            try
            {
                var process = Process.Start(psi);
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (NullReferenceException)
            {
                Logger.ErrorFormat("Failed to create the following profiler registration command '{0} {1}'", psi.FileName, psi.Arguments);
                throw new ExitApplicationWithoutReportingException();
            }
            catch (InvalidOperationException)
            {
                Logger.ErrorFormat("Failed to execute the following profiler registration command '{0} {1}'", psi.FileName, psi.Arguments);
                throw new ExitApplicationWithoutReportingException();
            }
        }

        private static void ExecuteRegsvr32(bool userRegistration, bool register, bool is64)
        {
            var startInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "regsvr32.exe"),
                                     string.Format("/s {2} {0} \"{1}\"",userRegistration ? UserRegistrationString : String.Empty,
                                     GetProfilerPath(is64), register ? string.Empty : "/u")) { CreateNoWindow = true, UseShellExecute = false };

            var exitCode = ExecuteProcess(startInfo);
            if (register && 0 != exitCode) // there is an oddity where unregistering the x64 version after the x86 (or vice versa) issues an access denied (5)
            {
                Logger.ErrorFormat("Failed to register(user:{0},register:{1},is64:{2}):{3} the profiler assembly; you may want to look into permissions or using the -register:user option instead. {4} {5}",
                        userRegistration, register, is64, exitCode, startInfo.FileName, startInfo.Arguments);
                throw new ExitApplicationWithoutReportingException();
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
        /// GetProfilerPath
        /// </summary>
        /// <param name="is64"></param>
        /// <returns></returns>
        public static string GetProfilerPath(bool is64)
        {
            return Path.Combine(GetAssemblyLocation(), is64 ? "x64" : "x86") + @"\OpenCover.Profiler.dll";
        }

        /// <summary>
        /// GetProfilerPath
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string GetProfilerPath(Registration registration)
        {
            switch (registration)
            {
                case Registration.Path32:
                    return GetProfilerPath(false);                    
                case Registration.Path64:
                    return GetProfilerPath(true);
                default:
                    return null;
            }
        }
    }
}
