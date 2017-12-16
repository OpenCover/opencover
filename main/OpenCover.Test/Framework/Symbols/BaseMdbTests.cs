using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework.Symbols
{
    public abstract class BaseMdbTests
    {

        protected Type TargetType = typeof(Moq.IMocked);
        protected string TargetAssembly = "Moq.dll";

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var assemblyPath = Path.GetDirectoryName(TargetType.Assembly.Location);

            var folder = Path.Combine(assemblyPath, "Mdb");
            var source = Path.Combine(assemblyPath, TargetAssembly);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, TargetAssembly);
            File.Copy(source, dest);
            File.Copy(Path.ChangeExtension(source, "pdb"), Path.ChangeExtension(dest, "pdb"));
            var process = new ProcessStartInfo
            {
                FileName = Path.Combine(assemblyPath, @"..\..\packages\Mono.pdb2mdb.0.1.0.20130128\tools\pdb2mdb.exe"),
                Arguments = dest,
                WorkingDirectory = folder,
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            var proc = Process.Start(process);
            proc.Do(_ => _.WaitForExit());

            Assert.IsTrue(File.Exists(dest + ".mdb"));
            File.Delete(Path.ChangeExtension(dest, "pdb"));
            Assert.IsFalse(File.Exists(Path.ChangeExtension(dest, "pdb")));
        }
    }
}