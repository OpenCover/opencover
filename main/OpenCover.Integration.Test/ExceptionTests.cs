using System;
using System.Collections;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using NUnit.Framework;
using OpenCover.Test.Integration;

namespace OpenCover.Integration.Test
{
    [TestFixture]
    public class ExceptionTests : ProfilerBaseFixture
    {
        public static IEnumerable SimpleExceptionTestMethods
        {
            get
            {
                return typeof(SimpleExceptionTests)
                    .GetMethods()
                    .Where(m => m.GetAttribute<TestAttribute>() != null)
                    .Select(m => new TestCaseData(m.Name).SetName(m.Name));
            }
        }

        [Test]
        [TestCaseSource(typeof(ExceptionTests), nameof(SimpleExceptionTestMethods))]
        public void Run_ExceptionTest(string testName)
        {
            ExecuteProfiler32((info) =>
            {
                info.FileName = Path.Combine(Environment.CurrentDirectory, TestRunner);
                info.Arguments = $"--test:{testName} OpenCover.Test.dll ";
                info.WorkingDirectory = Environment.CurrentDirectory;
            });
        }
    }
}