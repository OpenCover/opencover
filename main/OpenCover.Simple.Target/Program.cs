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
            for (var i=0;i<22;i++)
            {
                if (i == 10) continue;
                Console.WriteLine("{0}",i);
                if (i == 15) break;
                switch (i)
                {
                    case 0:
                    case 3:
                    case 7:
                    case 8:
                        Console.WriteLine("0000");
                        break;
                    case 5:
                        Console.WriteLine("5555");
                        break;
                    default:
                        break;

                }
            }
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
