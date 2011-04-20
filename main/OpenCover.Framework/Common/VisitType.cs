using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Common
{
    /// <summary>
    /// What type of instrumentation point
    /// </summary>
    public enum VisitType
    {
        /// <summary>
        /// Corresponds to a sequence point
        /// </summary>
        SequencePoint,

        /// <summary>
        /// Corresponds to a method enter point
        /// </summary>
        MethodEnter,

        /// <summary>
        /// Corresponds to a method leave point
        /// </summary>
        MethodLeave,
    }

}
