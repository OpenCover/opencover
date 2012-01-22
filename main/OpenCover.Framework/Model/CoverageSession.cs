//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A coverage session
    /// </summary>
    public class CoverageSession
    {
        public CoverageSession()
        {
            Modules = new Module[0];
        }

        /// <summary>
        /// A unique session identifier
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// A list of modules that have been profiled under the session
        /// </summary>
        public Module[] Modules { get; set; }
    }
}
