using System;
using System.IO;
using System.Reflection;

namespace OpenCover.ThirdParty.Signer
{
    class Program
    {

        static void Main(string[] args)
        {
			var assemblyLocation = Assembly.GetAssembly (typeof(Program)).Location;
			var assemblyFolder = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            var baseFolder = Path.Combine(assemblyFolder, "..", "..", "..");

            SignGendarme(baseFolder);
        }

        private static void SignGendarme(string baseFolder)
        {
            var targetDirectory = Path.Combine(baseFolder, GendarmeSigner.TargetFolder);
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if (GendarmeSigner.AlreadySigned(baseFolder))
            {
                Console.WriteLine("Gendarme Framework is already Signed");
                return;
            }

            Console.WriteLine("Signing Gendarme Framework");
            GendarmeSigner.SignGendarmeFramework(baseFolder);

            Console.WriteLine("Signing Gendarme Rules Maintainability");
            GendarmeSigner.SignGendarmeRulesMaintainability(baseFolder);
        }
    }
}
