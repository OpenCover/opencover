using System.IO;
using System.Linq;
using Mono.Cecil;

namespace OpenCover.ThirdParty.Signer
{
    internal static class GendarmeSigner
    {

        private const string GendarmeVersion = "2.11.0.20121120";

        private static readonly string AssemblyName = $"Mono.Gendarme.{GendarmeVersion}";

        public static readonly string TargetFolder = Path.Combine("..", "tools", "GendarmeSigned");
        private static readonly string SourceFolder = Path.Combine("packages", AssemblyName, "tools");
        private static readonly string StrongNameKey = Path.Combine("..", "build", "Version", "opencover.3rdparty.snk");


        public static bool AlreadySigned(string baseFolder)
        {
            var frameworkAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");
            return frameworkAssembly.AlreadySigned();
        }

        public static void SignGendarmeRulesMaintainability(string baseFolder)
        {
            var frameworkAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");
            using (var frameworkDefinition = AssemblyDefinition.ReadAssembly(frameworkAssembly))
            {
                var key = Path.Combine(baseFolder, StrongNameKey);
                var assembly = Path.Combine(baseFolder, SourceFolder, "Gendarme.Rules.Maintainability.dll");
                var newAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Rules.Maintainability.dll");

                assembly = Path.GetFullPath(assembly);
                newAssembly = Path.GetFullPath(newAssembly);

                File.Copy(assembly, newAssembly, true);
                using (var definition = AssemblyDefinition.ReadAssembly(assembly))
                {
                    BindAssemblyReference(definition, frameworkDefinition);
                    RebindMonoCecil(definition);
                    definition.SignFile(newAssembly, key);
                }
            }
        }

        public static void SignGendarmeFramework(string baseFolder)
        {
            var key = Path.Combine(baseFolder, StrongNameKey);
            var assembly = Path.Combine(baseFolder, SourceFolder, "Gendarme.Framework.dll");
            var newAssembly = Path.Combine(baseFolder, TargetFolder, "Gendarme.Framework.dll");

            assembly = Path.GetFullPath(assembly);
            newAssembly = Path.GetFullPath(newAssembly);

            using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
            {
                RebindMonoCecil(definition);
                definition.SignFile(newAssembly, key);
            }
        }

        private static void RebindMonoCecil(AssemblyDefinition definition)
        {
            var assembly = typeof(AssemblyDefinition).Assembly.Location;
            using (var monoCecilDefinition = AssemblyDefinition.ReadAssembly(assembly))
            {
                BindAssemblyReference(definition, monoCecilDefinition);
            }
        }

        private static void BindAssemblyReference(AssemblyDefinition sourceAssembly, AssemblyDefinition referenceAssembly)
        {
            var referenceAssemblyRef = AssemblyNameReference.Parse(referenceAssembly.Name.ToString());

            foreach (var typeReference in sourceAssembly.MainModule.GetTypeReferences())
            {
                if (typeReference.Scope.Name == referenceAssembly.Name.Name)
                {
                    typeReference.Scope = referenceAssemblyRef;
                }
            }

            var oldReference = sourceAssembly
                .MainModule.AssemblyReferences
                .FirstOrDefault(x => x.Name == referenceAssembly.Name.Name);

            if (oldReference != null)
            {
                sourceAssembly.MainModule.AssemblyReferences.Remove(oldReference);
                sourceAssembly.MainModule.AssemblyReferences.Add(referenceAssemblyRef);
            }
        }
    }
}