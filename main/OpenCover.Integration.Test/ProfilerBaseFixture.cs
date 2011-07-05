using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Manager;
using OpenCover.Framework.Persistance;

namespace OpenCover.Integration.Test
{

    [TestFixture]
    public abstract class ProfilerBaseFixture
    {
        private IFilter _filter;
        private ICommandLine _commandLine;
        private IPersistance _persistance;

        protected string TestTarget { get; set; }
 
        [SetUp]
        public void SetUp()
        {
            _filter = new Filter();
            _filter.AddFilter("-[mscorlib]*");
            _filter.AddFilter("-[System]*");
            _filter.AddFilter("-[System.*]*");
            _filter.AddFilter("-[Microsoft.VisualBasic]*");
            _filter.AddFilter("+[OpenCover.Samples.*]*");

            _commandLine = new Mock<ICommandLine>().Object;

            var filePersistance = new BasePersistance();
            _persistance = filePersistance;
        }

        protected void ExecuteProfiler32(Action<ProcessStartInfo> testProcess)
        {
            ProfilerRegistration.Register(true, false);
            ExecuteProfiler(Architecture.Arch32, testProcess);
            ProfilerRegistration.Unregister(true, false);
        }

        private void ExecuteProfiler(Architecture architecture, Action<ProcessStartInfo> testProcess)
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Initialise(_filter, _commandLine, _persistance);
            var harness = (IProfilerManager)bootstrapper.Container.Resolve(typeof(IProfilerManager), null);

            harness.RunProcess((environment) =>
            {
                var startInfo = new ProcessStartInfo();
                startInfo.EnvironmentVariables["Cor_Profiler"] = 
                                                   architecture == Architecture.Arch64
                                                       ? "{A7A1EDD8-D9A9-4D51-85EA-514A8C4A9100}"
                                                       : "{1542C21D-80C3-45E6-A56C-A9C1E4BEB7B8}";
                startInfo.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
                environment(startInfo.EnvironmentVariables);
                testProcess(startInfo);
                startInfo.UseShellExecute = false;
                var process = Process.Start(startInfo);
                process.WaitForExit();
            });
        }

    }
}
