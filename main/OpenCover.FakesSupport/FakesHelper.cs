using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenCover.FakesSupport
{
    public class FakesHelper
    {
        private const string CorEnableProfiling = "COR_ENABLE_PROFILING";
        private const string CorProfiler = "COR_PROFILER";
        private const string ChainExternalProfiler = "CHAIN_EXTERNAL_PROFILER";
        private const string ChainExternalProfilerLocation = "CHAIN_EXTERNAL_PROFILER_LOCATION";

        public static void LoadOpenCoverProfilerInstead(object data)
        {
            var dict = data as IDictionary<string, string>;
            if (dict == null) 
                return;

            if (!dict.ContainsKey(CorEnableProfiling) || dict[CorEnableProfiling] != "1") 
                return;

            var currentProfiler = dict[CorProfiler];
            var key = Registry.ClassesRoot.OpenSubKey(string.Format("CLSID\\{0}\\InprocServer32", currentProfiler));
            if (key == null)
                return;

            var location = key.GetValue(null) as string;
            dict[ChainExternalProfilerLocation] = location;

            dict[ChainExternalProfiler] = currentProfiler;
            dict[CorProfiler] = "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
        }

        public static void PretendWeLoadedFakesProfiler(object data)
        {
            //var args = data as string[];
            //foreach (var arg in args ?? new string[0])
            //{
            //    Console.WriteLine(arg);
            //}

            var enabled = Environment.GetEnvironmentVariable(CorEnableProfiling);
            if (enabled == "1")
            {
                var external = Environment.GetEnvironmentVariable(ChainExternalProfiler);
                Environment.SetEnvironmentVariable(CorProfiler, external);
            }
        }
    }
}
