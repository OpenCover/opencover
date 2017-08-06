using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A reference to a tracked method
    /// </summary>
    public class TrackedMethodRef
    {
        /// <summary>
        /// unique id assigned 
        /// </summary>
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }

        /// <summary>
        /// The visit count
        /// </summary>
        [XmlAttribute("vc")]
        public int VisitCount { get; set; }

    }


    /// <summary>
    /// A method being tracked
    /// </summary>
    [Serializable]
    public sealed class TrackedMethod
    {
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
        public string FullName { get; set; }

        /// <summary>
        /// The reason/plugin why the method is being tracked
        /// </summary>
        [XmlAttribute("strategy")]
        public string Strategy { get; set; }

    }
}
