using System;

namespace OpenCover.Support
{
    public class DomainHelper : IDomainHelper
    {
        public void AddResolveEventHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) => args.Name.StartsWith("OpenCover.Support, Version=1.0.0.0") ? System.Reflection.Assembly.GetExecutingAssembly() : null;
        }
    }
}