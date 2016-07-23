using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Ionic.Zip;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace OpenCover.Specs.Steps
{
    [Binding]
    public class PackagingSteps
    {
        [BeforeScenario]
        public void BeforeScenario()
        {
            var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            ScenarioContext.Current["assemblyPath"] = assemblyPath;
        }

        [AfterScenario("ziptag", "nugettag")]
        public void DeleteZipFolder()
        {
            var folder = (string)ScenarioContext.Current["targetFolder"];
            if (Directory.Exists(folder)) 
                Directory.Delete(folder, true);
        }

        [AfterScenario("msitag")]
        public void DeleteMsiFolder()
        {
            var folder = Path.GetFullPath(Path.Combine((string)ScenarioContext.Current["targetFolder"], ".."));
            if (Directory.Exists(folder)) 
                Directory.Delete(folder, true);
        }

        private static dynamic GetTargetPackage(string folder, string ext)
        {
            var files = Directory.EnumerateFiles(Path.Combine((string)ScenarioContext.Current["assemblyPath"], "..", "..", "..", "bin", folder), string.Format("*.{0}", ext));

            var target = files.Select(f => Regex.Match(f, string.Format(@".*\.(?<version>\d+\.\d+\.\d+)(-rc(?<revision>\d+))?\.{0}", ext)))
                 .Select(m => new { File = m.Value, Version = m.Groups["version"].Value, Revision = m.Groups["revision"].Value })
                 .Where(v => !string.IsNullOrEmpty(v.Version))
                 .OrderBy(v => new Version(string.Format("{0}.{1}", v.Version, string.IsNullOrEmpty(v.Revision) ? "0" : v.Revision)))
                 .LastOrDefault();

            return target;
        }

        [Given(@"I have a valid zip package in the output folder")]
        public void GivenIHaveAValidZipPackageInTheOutputFolder()
        {
            string targetFolder;
            string targetOutput;
            var targetFile = BuildTargets("zip", "zip", "zipFolder", "zipoutput.xml", out targetFolder, out targetOutput);

            ScenarioContext.Current["targetZip"] = targetFile;
            ScenarioContext.Current["targetFolder"] = targetFolder;
            ScenarioContext.Current["targetOutput"] = targetOutput;
        }

        private static dynamic BuildTargets(string folder, string ext, string dir, string xml, out string targetFolder, out string targetOutput)
        {
            var target = GetTargetPackage(folder, ext);

            Assert.NotNull(target, "Could not find a valid file.");

            var targetFile = Path.GetFullPath(target.File);
            targetFolder = Path.GetFullPath(Path.Combine((string)ScenarioContext.Current["assemblyPath"], dir));
            targetOutput = Path.GetFullPath(Path.Combine((string)ScenarioContext.Current["assemblyPath"], xml));

            if (File.Exists(targetOutput))
                File.Delete(targetOutput);
            return targetFile;
        }

        [Given(@"I (?:unzip|unpack) that package into a deployment folder")]
        public void GivenIUnzipThatPackageIntoADeploymentFolder()
        {
            var folder = (string)ScenarioContext.Current["targetFolder"];
            if (Directory.Exists(folder)) 
                Directory.Delete(folder, true);
            var zip = new ZipFile((string)ScenarioContext.Current["targetZip"]);
            zip.ExtractAll(folder);
            zip.Dispose();
        }

        [Given(@"I have a valid nugetpackage in the output folder")]
        public void GivenIHaveAValidNugetpackageInTheOutputFolder()
        {
            string targetFolder;
            string targetOutput;
            var targetFile = BuildTargets(@"packages\nuget\opencover", "nupkg", "nuFolder", "nuoutput.xml", out targetFolder, out targetOutput);

            ScenarioContext.Current["targetZip"] = targetFile;
            ScenarioContext.Current["targetFolder"] = targetFolder;
            ScenarioContext.Current["targetOutput"] = targetOutput;
        }

        [Given(@"I have a valid installer in the output folder")]
        public void GivenIHaveAValidInstallerInTheOutputFolder()
        {
            string targetFolder;
            string targetOutput;
            var targetFile = BuildTargets("installer", "msi", "msiFolder", "msioutput.xml", out targetFolder, out targetOutput);

            ScenarioContext.Current["targetMsi"] = targetFile;
            ScenarioContext.Current["targetFolder"] = targetFolder;
            ScenarioContext.Current["targetOutput"] = targetOutput;
        }

        [Given(@"I install that package into a deployment folder")]
        public void GivenIInstallThatPackageIntoADeploymentFolder()
        {
            var folder = (string)ScenarioContext.Current["targetFolder"];
            if (Directory.Exists(folder)) 
                Directory.Delete(folder, true);

            var installer = (string)ScenarioContext.Current["targetMsi"];
            var msiExec = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
            var startInfo = new ProcessStartInfo(msiExec)
            {
                Arguments = string.Format("/qn /a {0} TARGETDIR={1}", installer, folder),
                UseShellExecute = false
            };
            var process = Process.Start(startInfo);
            Assert.NotNull(process);
            process.WaitForExit();

            ScenarioContext.Current["targetFolder"] = Path.Combine(folder, "[ApplicationFolderName]");
        }

        [When(@"I execute the deployed OpenCover against the (x\d\d) target application")]
        public void WhenIExecuteTheDeployedOpenCoverAgainstTheXTargetApplication(string binFolder)
        {
            WhenIExecuteTheDeployedOpenCoverAgainstTheXTargetApplicationInSubfolder(binFolder, string.Empty);
        }

        [When(@"I execute the deployed OpenCover against the (x\d\d) target application in subfolder (.*)")]
        public void WhenIExecuteTheDeployedOpenCoverAgainstTheXTargetApplicationInSubfolder(string binFolder, string subfolder)
        {
            var folder = (string)ScenarioContext.Current["targetFolder"];
            var output = (string)ScenarioContext.Current["targetOutput"];

            var outputXml = string.Format(@"{0}\{1}_{2}{3}",
                Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output), binFolder, Path.GetExtension(output));

            if (File.Exists(outputXml)) 
                File.Delete(outputXml);

            var openCover = Path.Combine(folder, subfolder, "OpenCover.Console.exe");
            var target = Path.Combine(folder, string.Format(@"Samples\{0}\OpenCover.Simple.Target.exe", binFolder));
            var startInfo = new ProcessStartInfo(openCover)
            {
                Arguments = string.Format(@"-register:user ""-target:{0}"" ""-output:{1}""", target, outputXml),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = Process.Start(startInfo);
            Assert.NotNull(process);
            var console = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        [Then(@"the coverage results should be the same")]
        public void ThenTheCoverageResultsShouldBeTheSame()
        {
            const string summaryRegEx = @"(\<Summary\s.*?/\>)";
            const string seqPointRegEx = @"(\<SequencePoint\s.*?/\>)";
            const string branchPointRegEx = @"(\<BranchPoint\s.*?/\>)";
            const string methodPointRegEx = @"(\<MethodPoint\s.*?/\>)";

            var output = (string)ScenarioContext.Current["targetOutput"];

            var outputXml86 = string.Format(@"{0}\{1}_{2}{3}",
                Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output), "x86", Path.GetExtension(output));

            var outputXml64 = string.Format(@"{0}\{1}_{2}{3}",
                Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output), "x64", Path.GetExtension(output));

            Assert.IsTrue(File.Exists(outputXml86));
            Assert.IsTrue(File.Exists(outputXml64));

            var data86 = File.ReadAllText(outputXml86);
            var data64 = File.ReadAllText(outputXml64);

            // do both files have the same coverage
            var coverage86 = Utils.GetTotalCoverage(data86);
            var coverage64 = Utils.GetTotalCoverage(data64);
            Assert.AreEqual(decimal.Parse(coverage64), decimal.Parse(coverage86));
            Assert.Greater(decimal.Parse(coverage64), 0);

            // do both files have the same number of elements Summary and Sequence points and are their attributes the same
            CompareMatches(Regex.Matches(data64, summaryRegEx, RegexOptions.Multiline), Regex.Matches(data86, summaryRegEx, RegexOptions.Multiline));
            CompareMatches(Regex.Matches(data64, seqPointRegEx, RegexOptions.Multiline), Regex.Matches(data86, seqPointRegEx, RegexOptions.Multiline));
            CompareMatches(Regex.Matches(data64, branchPointRegEx, RegexOptions.Multiline), Regex.Matches(data86, branchPointRegEx, RegexOptions.Multiline));
            CompareMatches(Regex.Matches(data64, methodPointRegEx, RegexOptions.Multiline), Regex.Matches(data86, methodPointRegEx, RegexOptions.Multiline));
        }

        private static void CompareMatches(MatchCollection matches1, MatchCollection matches2)
        {
            Assert.AreEqual(matches1.Count, matches2.Count);
            for (var i = 0; i < matches1.Count; i++)
            {
                Assert.AreEqual(matches1[i].Value, matches2[i].Value);
            }
        }
    }
}
