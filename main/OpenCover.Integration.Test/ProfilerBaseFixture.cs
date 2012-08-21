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

    [TestFixture]
    public abstract class ProfilerBaseFixture
    {
        protected IFilter _filter;
        private Mock<ICommandLine> _commandLine;
        private Mock<ILog> _logger;
        private IPersistance _persistance;
        private IMemoryManager _manager;

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

            _commandLine = new Mock<ICommandLine>();
            _logger = new Mock<ILog>();

            var filePersistance = new BasePersistanceStub(_commandLine.Object, _logger.Object);
            _persistance = filePersistance;

            _manager = new MemoryManager();
        }

        protected void ExecuteProfiler32(Action<ProcessStartInfo> testProcess)
        {
            //ProfilerRegistration.Register(true);
            ExecuteProfiler(testProcess);
            //ProfilerRegistration.Unregister(true);
        }

        private void ExecuteProfiler(Action<ProcessStartInfo> testProcess)
        {
            var bootstrapper = new Bootstrapper(_logger.Object);
            bootstrapper.Initialise(_filter, _commandLine.Object, _persistance, _manager);
            var harness = (IProfilerManager)bootstrapper.Container.Resolve(typeof(IProfilerManager), null);

            harness.RunProcess((environment) =>
            {
                var startInfo = new ProcessStartInfo();
                environment(startInfo.EnvironmentVariables);
                testProcess(startInfo);
                startInfo.UseShellExecute = false;
                var process = Process.Start(startInfo);
                process.WaitForExit();
            }, false);
        }

    }
}
