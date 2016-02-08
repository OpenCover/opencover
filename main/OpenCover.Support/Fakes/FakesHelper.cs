using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenCover.Support.Fakes
{
    public static class FakesHelper
    {
        private const string CorEnableProfiling = "COR_ENABLE_PROFILING";
        private const string CorProfiler = "COR_PROFILER";
        private const string ChainExternalProfiler = "CHAIN_EXTERNAL_PROFILER";
        private const string ChainExternalProfilerLocation = "CHAIN_EXTERNAL_PROFILER_LOCATION";
        private const string OpenCoverProfilerGuid = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";

        public static void LoadOpenCoverProfilerInstead(object data)
        {
            var dict = data as IDictionary<string, string>;
            if (dict == null || IsAnotherProfilerAttached(dict)) 
                return;

            var currentProfiler = dict[CorProfiler];
            var key = Registry.ClassesRoot.OpenSubKey(string.Format("CLSID\\{0}\\InprocServer32", currentProfiler));
            if (key == null)
                return;

            var location = key.GetValue(null) as string;
            dict[ChainExternalProfilerLocation] = location;

            dict[ChainExternalProfiler] = currentProfiler;
            dict[CorProfiler] = OpenCoverProfilerGuid;
        }

        private static bool IsAnotherProfilerAttached(IDictionary<string, string> dict)
        {
            if (!dict.ContainsKey(CorEnableProfiling) || dict[CorEnableProfiling] != "1")
                return true;

            if (!dict.ContainsKey(CorProfiler) || dict[CorProfiler] == OpenCoverProfilerGuid)
                return true;

            return false;
        }

        public static void PretendWeLoadedFakesProfiler(object data)
        {
            var enabled = Environment.GetEnvironmentVariable(CorEnableProfiling);
            var profiler = Environment.GetEnvironmentVariable(CorEnableProfiling) ?? string.Empty;
            var external = Environment.GetEnvironmentVariable(ChainExternalProfiler);
            if (enabled == "1" && !string.IsNullOrEmpty(external) && 
                !profiler.Equals(OpenCoverProfilerGuid, StringComparison.InvariantCultureIgnoreCase))
            {
                Environment.SetEnvironmentVariable(CorProfiler, external);
            }
        }
    }
}
