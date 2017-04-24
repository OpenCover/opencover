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
            var crashReporterAssembly = Path.Combine(baseFolder, TargetFolder, "CrashReporter.NET.dll");
            return crashReporterAssembly.AlreadySigned();
        }

        public static void SignAssembly(string baseFolder)
        {
            var key = Path.Combine(baseFolder, StrongNameKey);
            var assembly = Path.Combine(baseFolder, SourceFolder, "CrashReporter.NET.dll");
            var newAssembly = Path.Combine(baseFolder, TargetFolder, "CrashReporter.NET.dll");

            assembly = Path.GetFullPath(assembly);
            newAssembly = Path.GetFullPath(newAssembly);

            using (var definition = AssemblyDefinition.ReadAssembly(assembly))
            {
                definition.SignFile(newAssembly, key);
            }
        }
    }
}