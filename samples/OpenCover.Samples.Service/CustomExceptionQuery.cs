using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCover.Samples.Framework;

namespace OpenCover.Samples.Service
{
    class CustomExceptionQuery : ITestExceptionQuery
    {
        public bool ThrowException()
        {
            return true;
        }

        public void InFinally()
        {            
        }

        public void InFault()
        {
        }

        public void InException(Exception ex)
        {
        }

        public void InFilter()
        {
        }
    }
}
