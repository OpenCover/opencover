using System;
using System.Threading.Tasks;

namespace Target
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Person p = new Person();
            while (true)
            {
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 2; i++)
                    {
                        p.Age = 1;
                    }
                });
                string s = Console.ReadLine();
                if (s == "q")
                {
                    break;
                }
            }
        }
    }
}
