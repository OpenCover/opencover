using System;
using System.IO;
using NUnit.Framework;

namespace OpenCover.Integration.Test
{
    [TestFixture]
    public class BranchTests : ProfilerBaseFixture
    {
        [Test]
        public void Execute_SimpleIf()
        {
            _filter.AddFilter("+[OpenCover.Test]OpenCover.Test.Integration.SimpleBranchTests*");
            ExecuteProfiler32((info) =>
                                  {
                                      info.FileName = Path.Combine(Environment.CurrentDirectory, TestRunner);
                                      info.Arguments = "--test:OpenCover.Test.Integration.SimpleBranchTests.SimpleIf OpenCover.Test.dll";
                                      info.WorkingDirectory = Environment.CurrentDirectory;
                                  });
        }
    }
}