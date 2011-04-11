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
            for (var i=0;i<5;i++)
            {
                Console.WriteLine("{0}",i);
            }
        }


        static int ThrowException()
        {
            try
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
            }
            return 0;
        }

        static void ThrowException2()
        {
            try
            {
                Console.WriteLine("Y0");
            }
            finally 
            {
                Console.WriteLine("Y1");
            }

            try
            {
                Console.WriteLine("Y00");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                Console.WriteLine("Y10");
                throw;
            }
            catch (Exception)
            {
                Console.WriteLine("Y12");
                throw;
            }

            try
            {
                Console.WriteLine("Y20");
                try
                {
                    Console.WriteLine("Y011");
                }
                finally
                {
                    Console.WriteLine("Y010");
                }
            }
            finally
            {
                Console.WriteLine("Y21");
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
