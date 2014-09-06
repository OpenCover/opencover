//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Specialized;

namespace OpenCover.Framework.Manager
{
    /// <summary>
    /// Describe the external interaction with the profiler manager
    /// </summary>
    public interface IProfilerManager
    {
        /// <summary>
        /// Let's start profiling
        /// </summary>
        /// <param name="process"></param>
        /// <param name="servicePrincipal"></param>
        void RunProcess(Action<Action<StringDictionary>> process, string[] servicePrincipal);
    }
}