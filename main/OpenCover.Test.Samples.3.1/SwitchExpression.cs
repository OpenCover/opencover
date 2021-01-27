using System;

namespace OpenCover.Test.Samples._3._1
{
    public enum MyTest
    {
        A,
        B
    }

    public static class SwitchExpression
    {
        /// <summary>
        /// Issue 960 - The compiler creates unusual IL with a conditional branches that only has nop instructions, 
        /// no obvious ways to exercise the path and has then inserted a sequence point to boot
        /// </summary>
        public static string MapMyTest(this MyTest myTest)
        {
            return myTest switch
            {
                MyTest.A => "a",
                MyTest.B => "b",
                _ => string.Empty,
            };
        }
    }
}
