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
using OpenCover.Framework.Utility;
using log4net;

namespace OpenCover.Integration.Test
{
    internal class BasePersistanceStub : BasePersistance
    {
        public BasePersistanceStub(ICommandLine commandLine, ILog logger) : base(commandLine, logger)
        {
        }

        public override void Commit()
        {
            //throw new NotImplementedException();
        }
    }

    public abstract class ProfilerBaseFixture
    {
        protected IFilter _filter;
        private Mock<ICommandLine> _commandLine;
        private Mock<ILog> _logger;
        private IPersistance _persistance;

        protected string TestTarget { get; set; }

        protected string TestRunner
        {
            get { return @"..\..\..\main\packages\NUnit.ConsoleRunner.3.12.0\tools\nunit3-console.exe"; }
        }

        [SetUp]
        public void SetUp()
        {
            _filter = new Filter(false);
            _filter.AddFilter("-[mscorlib]*");
            _filter.AddFilter("-[System]*");
            _filter.AddFilter("-[System.*]*");
            _filter.AddFilter("-[Microsoft.VisualBasic]*");
            _filter.AddFilter("+[OpenCover.Samples.*]*");

            _commandLine = new Mock<ICommandLine>();
            _logger = new Mock<ILog>();

            var filePersistance = new BasePersistanceStub(_commandLine.Object, _logger.Object);
            _persistance = filePersistance;

        }

        protected void ExecuteProfiler32(Action<ProcessStartInfo> testProcess)
        {
            //ProfilerRegistration.Register(true);
            ExecuteProfiler(testProcess);
            //ProfilerRegistration.Unregister(true);
        }

        private void ExecuteProfiler(Action<ProcessStartInfo> testProcess)
        {
            using (var bootstrapper = new Bootstrapper(_logger.Object))
            {
                bootstrapper.Initialise(_filter, _commandLine.Object, _persistance, new NullPerfCounter());
                var harness = bootstrapper.Resolve<IProfilerManager>();

                harness.RunProcess((environment) =>
                {
                    var startInfo = new ProcessStartInfo();
                    environment(startInfo.EnvironmentVariables);
                    testProcess(startInfo);
                    startInfo.UseShellExecute = false;
                    var process = Process.Start(startInfo);
                    process.WaitForExit();
                }, Enumerable.Empty<string>().ToArray());
            }
        }

    }
}
