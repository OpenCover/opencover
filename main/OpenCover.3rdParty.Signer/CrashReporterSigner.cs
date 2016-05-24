using System.IO;
using Mono.Cecil;

namespace OpenCover.ThirdParty.Signer
{
    internal static class CrashReporterSigner
    {

        private const string Version = "1.5";

        private static readonly string AssemblyName = $"CrashReporterdotNet.{Version}";

        public static readonly string TargetFolder = Path.Combine("..", "tools", "CrashReporterSigned");
        private static readonly string SourceFolder = Path.Combine("packages", AssemblyName, "lib", "net20");
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
                    return false;
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
            
            definition.SignFile(newAssembly, key);
        }
    }
}