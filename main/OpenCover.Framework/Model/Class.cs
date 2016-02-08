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
    public class Class : SummarySkippedEntity
    {
        /// <summary>
        /// instantiate
        /// </summary>
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

        /// <summary>
        /// If a class was skipped by instrumentation, supply the reason why
        /// </summary>
        /// <param name="reason"></param>
        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
        }
    }
}
