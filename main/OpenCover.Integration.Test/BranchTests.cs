using System;
using System.IO;
using NUnit.Framework;

namespace OpenCover.Integration.Test
{
    public class BranchTests : ProfilerBaseFixture
    {
        [Test]
        public void Execute_SimpleIf()
        {
            _filter.AddFilter("+[OpenCover.Test]OpenCover.Test.Integration.SimpleBranchTests*");
            ExecuteProfiler32((info) =>
                                  {
                                      info.FileName = Path.Combine(Environment.CurrentDirectory,
                                                                   @"..\..\..\tools\NUnit-2.5.9.10348\bin\net-2.0\nunit-console-x86.exe");
                                      info.Arguments =
                                          "OpenCover.Test.dll /noshadow /run:OpenCover.Test.Integration.SimpleBranchTests.SimpleIf";
                                      info.WorkingDirectory = Environment.CurrentDirectory;
                                  });
        }
    }
}