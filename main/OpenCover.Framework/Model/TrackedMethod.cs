using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    public class TrackedMethodRef
    {
        /// <summary>
        /// unique id assigned 
        /// </summary>
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }

        // visit count
        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

    }


    /// <summary>
    /// A method being tracked
    /// </summary>
    public class TrackedMethod
    {
        private static int _methodId;

        public TrackedMethod()
        {
            UniqueId = (UInt32)Interlocked.Increment(ref _methodId);
        }

        /// <summary>
        /// unique id assigned 
        /// </summary>
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }

        /// <summary>
        /// The MetadataToken used to identify this entity within the assembly
        /// </summary>
        [XmlAttribute("token")]
        public int MetadataToken { get; set; }

        /// <summary>
        /// The name of the method being tracked
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

    }
}
