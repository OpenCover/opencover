//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework
{
    /// <summary>
    /// properties exposed by the command line object for use in other entities
    /// </summary>
    public interface ICommandLine
    {
        /// <summary>
        /// the target directory
        /// </summary>
        string TargetDir { get; }

        /// <summary>
        /// If specified then results to be merged by matching hash 
        /// </summary>
        bool MergeByHash { get; }

        /// <summary>
        /// Show the unvisited classes/methods at the end of the coverage run
        /// </summary>
        bool ShowUnvisited { get; }

        /// <summary>
        /// 
        /// </summary>
        List<SkippedMethod> HideSkipped { get; }
    }
}