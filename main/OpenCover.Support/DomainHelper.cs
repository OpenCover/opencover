using System;
using System.Reflection;

namespace OpenCover.Support
{
    public class DomainHelper : IDomainHelper
    {
        public void AddResolveEventHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return args.Name.StartsWith("OpenCover.Support, Version=1.0.0.0") ? Assembly.GetExecutingAssembly() : null;
        }
    }
}