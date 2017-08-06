using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace OpenCover.ThirdParty.Signer
{
    internal static class SigningExtensions
    {
        public static void SignFile(this AssemblyDefinition definition, string outputPath, string key)
        {
            using (var stream = new FileStream(key, FileMode.Open, FileAccess.Read))
            {
                var keyPair = new StrongNameKeyPair(stream);
                definition.Write(outputPath, new WriterParameters
                {
                    StrongNameKeyPair = keyPair
                });
            }
        }

        public static bool AlreadySigned(this string assemblyPath)
        {
            if (File.Exists(assemblyPath))
            {
                try
                {
                    using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
                    {
                        return assemblyDefinition.Name.HasPublicKey;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}