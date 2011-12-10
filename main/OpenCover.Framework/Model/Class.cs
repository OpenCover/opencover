//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class
    {
        public Class()
        {
            Methods = new Method[0];
        }

        /// <summary>
        /// The full name of the class
        /// </summary>
        public string FullName { get; set; }
        
        [XmlIgnore]
        internal File[] Files { get; set; }

        /// <summary>
        /// A list of methods that make up the class
        /// </summary>
        public Method[] Methods { get; set; }

        private SkippedMethod? skippedDueTo;

        /// <summary>
        /// If this class has been skipped then this value will describe why
        /// </summary>
        [XmlAttribute("skippedDueTo")]
        public SkippedMethod SkippedDueTo
        {
            get { return skippedDueTo.GetValueOrDefault(); }
            set { skippedDueTo = value; }
        }

        /// <summary>
        /// If this class has been skipped then this value will allow the data to be serialized
        /// </summary>
        public bool ShouldSerializeSkippedDueTo() { return skippedDueTo.HasValue; }

    }
}
