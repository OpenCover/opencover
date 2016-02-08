using System.IO;
using System.Linq;
using Mono.Cecil;

namespace OpenCover.ThirdParty.Signer
{
    internal static class GendarmeSigner
    {

        private const string GendarmeVersion = "2.11.0.20121120";

        private static readonly string GendarmeAssemblyName = string.Format("Mono.Gendarme.{0}", GendarmeVersion);

        public static readonly string TargetFolder = Path.Combine("..", "tools", "GendarmeSigned");
        private static readonly string SourceFolder = Path.Combine("packages", GendarmeAssemblyName, "tools");
        private static readonly string StrongNameKey = Path.Combine("..", "build", "Version", "opencover.3rdparty.snk");


        public static bool AlreadySigned(string baseFolder)
        {
            var frameworkAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");
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

        public static void SignGendarmeRulesMaintainability(string baseFolder)
        {
            var frameworkAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");
            var frameworkDefinition = AssemblyDefinition.ReadAssembly(frameworkAssembly);
            var frameworkAssemblyRef = AssemblyNameReference.Parse(frameworkDefinition.Name.ToString());

            var key = Path.Combine(baseFolder, StrongNameKey);
            var assembly = Path.Combine(baseFolder, SourceFolder, "Gendarme.Rules.Maintainability.dll");
            var newAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Rules.Maintainability.dll");

            assembly = Path.GetFullPath(assembly);
            newAssembly = Path.GetFullPath(newAssembly);

            File.Copy(assembly, newAssembly, true);
            var definition = AssemblyDefinition.ReadAssembly(newAssembly);

            // update all type references to the now signed base assembly
            foreach (var typeReference in definition.MainModule.GetTypeReferences())
            {
                if (typeReference.Scope.Name == frameworkDefinition.Name.Name)
                {
                    typeReference.Scope = frameworkAssemblyRef;
                }
            }

            // update assembly references to use the now signed base assembly
            var oldReference = definition.MainModule.AssemblyReferences.FirstOrDefault(x => x.Name == frameworkDefinition.Name.Name);
            if (oldReference != null)
            {
                definition.MainModule.AssemblyReferences.Remove(oldReference);
                definition.MainModule.AssemblyReferences.Add(frameworkAssemblyRef);
            }

            definition.SignFile(newAssembly, key);
        }

        public static void SignGendarmeFramework(string baseFolder)
        {
            var key = Path.Combine(baseFolder, StrongNameKey);
            var assembly = Path.Combine(baseFolder, SourceFolder, "Gendarme.Framework.dll");
            var newAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");

            assembly = Path.GetFullPath(assembly);
            newAssembly = Path.GetFullPath(newAssembly);

            File.Copy(assembly, newAssembly, true);
            var definition = AssemblyDefinition.ReadAssembly(newAssembly);

            definition.SignFile(newAssembly, key);
        }
    }
}