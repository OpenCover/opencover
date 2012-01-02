//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Specialized;

namespace OpenCover.Framework.Manager
{
    public interface IProfilerManager
    {
        void RunProcess(Action<Action<StringDictionary>> process, bool isService);
    }
}