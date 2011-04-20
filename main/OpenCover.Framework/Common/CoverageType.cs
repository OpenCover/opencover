using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Common
{
    ///<summary>
    /// The coverage combinations
    ///</summary>
    [Flags]
    public enum CoverageType
    {
        ///<summary>
        /// No Coverage - do nothing
        ///</summary>
        None = 0,
        ///<summary>
        /// Sequence point coverage i.e. statement coverage
        ///</summary>
        Sequence = 1,
        ///<summary>
        /// Method coverage i.e. method enter and exit
        ///</summary>
        Method = 2,
        ///<summary>
        /// Branch coverage i.e. are all paths exercised (but not necessarily all combinations of paths)
        ///</summary>
        Branch = 4
    }
}
