using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace OpenCover.ThirdParty.Signer
{
    internal static class CrashReporterSigner
    {

        private const string Version = "1.5";

        private static readonly string GendarmeAssemblyName = string.Format("CrashReporterdotNet.{0}", Version);

        public static readonly string TargetFolder = Path.Combine("..", "tools", "CrashReporterSigned");
        private static readonly string SourceFolder = Path.Combine("packages", GendarmeAssemblyName, "lib", "net20");
        private static readonly string StrongNameKey = Path.Combine("..", "build", "Version", "opencover.3rdparty.snk");


        public static bool AlreadySigned(string baseFolder)
        {
            var frameworkAssembly = Path.Combine(baseFolder, TargetFolder, "CrashReporter.NET.dll");
            if (File.Exists(frameworkAssembly))
            {
                try
                {
                    var frameworkDefinition = AssemblyDefinition.ReadAssembly(frameworkAssembly);
                    return frameworkDefinition.Name.HasPublicKey;
                }
                catch
                {
                }
            }
            return false;
        }

        public static void SignAssembly(string baseFolder)
        {
            var key = Path.Combine(baseFolder, StrongNameKey);
            var assembly = Path.Combine(baseFolder, SourceFolder, "CrashReporter.NET.dll");
            var newAssembly = Path.Combine(baseFolder, TargetFolder, "CrashReporter.NET.dll");

            assembly = Path.GetFullPath(assembly);
            newAssembly = Path.GetFullPath(newAssembly);

            File.Copy(assembly, newAssembly, true);
            var definition = AssemblyDefinition.ReadAssembly(newAssembly);
            var keyPair = new StrongNameKeyPair(new FileStream(key, FileMode.Open, FileAccess.Read));
            definition.Write(newAssembly, new WriterParameters() { StrongNameKeyPair = keyPair });
        }
    }
}