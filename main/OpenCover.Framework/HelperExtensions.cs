using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework
{
    static class HelperExtensions
    {
        public static TRet Maybe<T, TRet>(this T value, Func<T, TRet> action, TRet defValue = default(TRet))
            where T : class
        {
            return (value != null) ? action(value) : defValue;
        }
    }
}
