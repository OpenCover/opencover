using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Temp
{
    class PrivateClass
    {
        private PrivateClass()
        {
            
        }
    }

    struct PrivateStruct
    {
        private void Method()
        {
            
        }
    }

    abstract public class AbstractClass
    {
    
        public void Method()
        {
            
        }
    }

    public class PublicClass : AbstractClass
    {

        new public void Method()
        {

        }
    }
}
