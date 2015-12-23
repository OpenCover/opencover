using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Integration
{
    /// <summary>
    /// Replicates issue with threads as reported in issue #366
    /// </summary>
    [TestFixture]
    public class ThreadingTests
    {
        const int NB_THREADS = 50;
        static readonly ManualResetEvent[] ResetEvents = new ManualResetEvent[NB_THREADS];

        [Test]
        public void RunManyThreads()
        {
            //Thread.Sleep(15000);
            for (int i = 0; i < NB_THREADS; i++)
            {
                ResetEvents[i] = new ManualResetEvent(false);
                new Thread(DoWork).Start(ResetEvents[i]);
            }
            var chrono = Stopwatch.StartNew();
            long n = 0;
            while (n < 2000)
            {
                if (++n % 200 == 0)
                    Console.WriteLine(n.ToString());
                var current = WaitHandle.WaitAny(ResetEvents.ToArray<WaitHandle>());
                ResetEvents[current].Reset();
                new Thread(DoWork).Start(ResetEvents[current]);
            }
            Console.WriteLine("Took {0} seconds", chrono.Elapsed.TotalSeconds);
            Assert.Pass();
        }

        public static void DoWork(object o)
        {
            var resetEvent = (ManualResetEvent)o;
            resetEvent.Do(re =>
            {
                var rnd = new Random();
                double res = 0;
                for (var i = 0; i < 10000; i++)
                    res += rnd.NextDouble();
                re.Set();
            });

           
        }
        
    }
}
