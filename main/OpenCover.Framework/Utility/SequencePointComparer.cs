/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 8.1.2016.
 * Time: 10:41
 * 
 */
using System;
using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// Description of SequencePointComparer.
    /// </summary>
    public class SequencePointComparer : EqualityComparer<SequencePoint>
    {
        #region implemented abstract members of EqualityComparer

        /// <summary>
        /// Implements IEqualityComparer&lt;SequencePoint&gt;.Equals
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals(SequencePoint x, SequencePoint y)
        {
			return (ReferenceEquals(x, y) 
			        || (x.IsFileIdEqual (y) && x.IsPositionEqual (y))
			       );
        }

        /// <summary>
        /// Implements IEqualityComparer&lt;SequencePoint&gt;.Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int GetHashCode(SequencePoint obj)
        {
            return unchecked ((int)obj.FileId << 4) ^ unchecked(obj.StartLine << 3) ^ unchecked (obj.EndLine << 2) ^ unchecked (obj.StartColumn << 1) ^ (obj.EndColumn);
        }

        #endregion
    }
}
