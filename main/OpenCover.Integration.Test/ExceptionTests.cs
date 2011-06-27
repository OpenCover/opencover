using System;
using System.IO;
using NUnit.Framework;

namespace OpenCover.Integration.Test
{
    public class ExceptionTests : ProfilerBaseFixture
    {
        [Test]
        public void TryFault_NoExceptionThrown()
        {
            ExecuteProfiler32((info) =>
            {
                info.FileName = Path.Combine(Environment.CurrentDirectory,
                    @"..\..\..\tools\NUnit-2.5.9.10348\bin\net-2.0\nunit-console-x86.exe");
                info.Arguments = "OpenCover.Test.dll /noshadow /run:OpenCover.Test.Integration.SimpleExceptionTests.TryFault_NoExceptionThrown";
                info.WorkingDirectory = Environment.CurrentDirectory;
            });
        }

        [Test]
        public void TryFinally_NoExceptionThrown()
        {
            ExecuteProfiler32((info) =>
            {
                info.FileName = Path.Combine(Environment.CurrentDirectory,
                    @"..\..\..\tools\NUnit-2.5.9.10348\bin\net-2.0\nunit-console-x86.exe");
                info.Arguments = "OpenCover.Test.dll /noshadow /run:OpenCover.Test.Integration.SimpleExceptionTests.TryFinally_NoExceptionThrown";
                info.WorkingDirectory = Environment.CurrentDirectory;
            });
        }

        [Test]
        public void TryFilter_NoExceptionThrown()
        {
            ExecuteProfiler32((info) =>
            {
                info.FileName = Path.Combine(Environment.CurrentDirectory,
                    @"..\..\..\tools\NUnit-2.5.9.10348\bin\net-2.0\nunit-console-x86.exe");
                info.Arguments = "OpenCover.Test.dll /noshadow /run:OpenCover.Test.Integration.SimpleExceptionTests.TryFilter_NoExceptionThrown";
                info.WorkingDirectory = Environment.CurrentDirectory;
            });
        }

    }
}