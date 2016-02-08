//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// The details of a module
    /// </summary>
    public class Module : SummarySkippedEntity
    {
        /// <summary>
        /// simple constructor
        /// </summary>
        public Module()
        {
            Aliases = new List<string>();
        }

        /// <summary>
        /// The full path name to the module
        /// </summary>
        public string ModulePath { get; set; }

        /// <summary>
        /// GetlastWriteTimeUtc
        /// </summary>
        public DateTime ModuleTime { get; set; }

        /// <summary>
        /// A list of aliases
        /// </summary>
        [XmlIgnore]
        public IList<string> Aliases { get; private set; } 

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

        /// <summary>
        /// Methods that are being tracked i.e. test methods
        /// </summary>
        public TrackedMethod[] TrackedMethods { get; set; }

        /// <summary>
        /// A hash of the file used to group them together (especially when running against mstest)
        /// </summary>
        [XmlAttribute("hash")]
        public string ModuleHash { get; set; }

        /// <summary>
        /// Mark an entity as skipped
        /// </summary>
        /// <param name="reason">Provide a reason</param>
        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
        }
    }
}
