using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCover.Samples.Framework;

namespace OpenCover.Samples.CS
{
    public class TryFinallyTarget
    {
        private readonly ITestExceptionQuery _query;

        public TryFinallyTarget(ITestExceptionQuery query)
        {
            _query = query;
        }

        public void TryFinally()
        {
            try
            {
                _query.ThrowException();
            }
            finally
            {
                _query.InFinally();
            }

        }
    }
}
