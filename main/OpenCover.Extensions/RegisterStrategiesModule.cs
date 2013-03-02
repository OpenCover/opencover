using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using OpenCover.Extensions.Strategy;
using OpenCover.Framework.Strategy;

namespace OpenCover.Extensions
{
    public class RegisterStrategiesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TrackNUnitTestMethods>().As<ITrackedMethodStrategy>();
            builder.RegisterType<TrackMSTestTestMethods>().As<ITrackedMethodStrategy>();
            builder.RegisterType<TrackXUnitTestMethods>().As<ITrackedMethodStrategy>();
            base.Load(builder);
        }

    }
}
