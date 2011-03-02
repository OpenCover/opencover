using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Simple.Target
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            new Class();
            new GenericClass<object>();
        }
    }

    public class Class
    {
        
    }

    public class GenericClass<T>
    {
        
    }
}
