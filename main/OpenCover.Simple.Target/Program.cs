using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenCover.Simple.Target
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World *****");
            Thread.Sleep(2000);
            var o = new Namespace.Other();
            o.Data = 22;
            new Class();
            new GenericClass<object>();
            o.Data = o.Data + 1;
        }
    }

    public class Class
    {
        public Class()
        {
        }
    }

    public class GenericClass<T>
    {
        private int i;
        public GenericClass()
        {
            i = 0;
        }
    }
}
