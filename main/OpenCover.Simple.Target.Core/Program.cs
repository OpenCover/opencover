using System;

namespace OpenCover.Simple.Target
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        delegate T SelfApplicable<T>(SelfApplicable<T> self);

        static void Main(string[] args)
        {
            // The Y combinator
            SelfApplicable<Func<Func<Func<int, int>, Func<int, int>>, Func<int, int>>> Y = y => f => x => f(y(y)(f))(x);

            // The fixed point generator
            var Fix = Y(Y);

            // The higher order function describing factorial
            Func<Func<int, int>, Func<int, int>> F = fac => x => x == 0 ? 1 : x * fac(x - 1);

            // The factorial function itself
            var factorial = Fix(F);

            for (int j = 0; j < 1000; j++)
            {
                for (var i = 0; i < 12; i++)
                {
                    Console.WriteLine(factorial(i));
                }    
            }
            
        }
    }
}
