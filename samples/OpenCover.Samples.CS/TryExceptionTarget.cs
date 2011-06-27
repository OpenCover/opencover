using System;
using OpenCover.Samples.Framework;

namespace OpenCover.Samples.CS
{
    public class TryExceptionTarget
    {
        private readonly ITestExceptionQuery _query;

        public TryExceptionTarget(ITestExceptionQuery query)
        {
            _query = query;
        }

        public void TryException()
        {
            try
            {
                _query.ThrowException();
            }
            catch(Exception ex)
            {
                _query.InException(ex);
                throw;
            }

        }
    }
}