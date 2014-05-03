using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCover.Test.Service
{
    /// <summary>
    /// Trival service based on the walkthrough at http://msdn.microsoft.com/en-us/library/zt39148a%28v=vs.110%29.aspx
    /// Uses Debug.WriteLine rather than Event Log to avoid the need to set access rights for the relevant keys
    /// Follow the service activity by running DebugView -- http://technet.microsoft.com/en-gb/sysinternals/bb896647.aspx -- as 
    /// Administrator and Capture Global Win32 traces
    /// </summary>
    public partial class Service : ServiceBase
    {
        private AutoResetEvent waiter;

        /// <summary>
        /// Initializes a new instance of the Service class
        /// </summary>
        public Service()
        {
            InitializeComponent();
            waiter = new AutoResetEvent(false);
            CanShutdown = true;
            CanStop = true;
            Debug.WriteLine("Constructed service");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Service() 
            };

            ServiceBase.Run(ServicesToRun);
        }

        /// <summary>
        /// Called by the SCM when a service start is sent and starts 
        /// the service payload as an asynchronous operation
        /// </summary>
        /// <param name="args">This parameter is not used.</param>
        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("Starting service");
            Task.Run(() =>
            {
                // Heartbeat at 5 second intervals until signalled to stop
                var interval = new TimeSpan(0, 0, 5);
                while (!waiter.WaitOne(interval))
                {
                    Debug.WriteLine("Service working");
                }

                Debug.WriteLine("Service exiting");
            });
        }

        /// <summary>
        /// Called by the SCM when a service stop is sent
        /// </summary>
        protected override void OnStop()
        {
            Debug.WriteLine("Stopping service");
            waiter.Set();
        }

        /// <summary>
        /// Called by the SCM when the system is shutting down
        /// </summary>
        protected override void OnShutdown()
        {
            Debug.WriteLine("Shutting down service");
            waiter.Set();
        }
    }
}
