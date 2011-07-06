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
    /// The details of a module
    /// </summary>
    public class Module
    {
        /// <summary>
        /// The full path name to the module
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// The name of the module
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// The files that make up the module
        /// </summary>
        public File[] Files { get; set; }

        /// <summary>
        /// The classes that make up the module
        /// </summary>
        public Class[] Classes { get; set; }
    }
}
