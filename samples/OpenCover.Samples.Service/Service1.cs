using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using OpenCover.Samples.CS;

namespace OpenCover.Samples.Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var target = new TryFinallyTarget(new CustomExceptionQuery());
            target.TryFinally();
        }

        protected override void OnStop()
        {
            var target = new TryExceptionTarget(new CustomExceptionQuery());
            target.TryException();
        }
    }
}
