using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Samples.Framework
{
    public interface ITestExceptionQuery
    {
        bool ThrowException();
        void InFinally();
        void InFault();
        void InException(Exception ex);
        void InFilter();
    }
}
