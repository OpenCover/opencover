using System;

namespace OpenCover.FakesSupport
{
    public class FakesDomainHelper : IFakesDomainHelper
    {
        public void AddResolveEventHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) => args.Name.StartsWith("OpenCover.FakesSupport, Version=1.0.0.0") ? System.Reflection.Assembly.GetExecutingAssembly() : null;
        }
    }
}