using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace OpenCover.Specs.Steps
{
    [Binding]
    public class DotNetCoreSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public DotNetCoreSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I can find the OpenCover application")]
        public void GivenICanFindTheOpenCoverApplication()
        {
#if DEBUG
            var targetFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\bin\Debug"));
#else
            var targetFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\bin\Release"));
#endif
            Assert.IsTrue(File.Exists(Path.Combine(targetFolder, "OpenCover.Console.exe")));

            _scenarioContext["TargetFolder"] = targetFolder;
        }

        [Given(@"I can find the target \.net core '(.*)' application '(.*)'")]
        public void GivenICanFindTheTarget_NetCoreApplication(string version, string application)
        {
#if DEBUG
            var targetPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", $@"..\..\..\{application}\bin\Debug\netcoreapp{version}"));
#else
            var targetPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", $@"..\..\..\{application}\bin\Release\netcoreapp{version}"));
#endif
            var targetApp = Directory.EnumerateFiles(targetPath, $"{application}.dll", SearchOption.AllDirectories).FirstOrDefault();

            Console.WriteLine($"Found target application in '{targetApp}'");

            Assert.IsTrue(File.Exists(targetApp));

            _scenarioContext["TargetApp"] = targetApp;
        }

        [When(@"I execute OpenCover against the target application using the switch '(.*)'")]
        public void WhenIExecuteOpenCoverAgainstTheTargetApplicationUsingTheSwitch(string additionalSwitch)
        {
            var dotnetexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\dotnet.exe");
            var targetApp = (string)_scenarioContext["TargetApp"];
            var targetFolder = (string)_scenarioContext["TargetFolder"];
            var outputXml = Path.Combine(Path.GetDirectoryName(targetApp) ?? ".", "results.xml");
            if (File.Exists(outputXml))
                File.Delete(outputXml);

            var info = new ProcessStartInfo
            {
                FileName = Path.Combine(targetFolder, "OpenCover.Console.exe"),
                Arguments = $"{additionalSwitch ?? ""} -register:user \"-target:{dotnetexe}\" \"-targetargs:{targetApp}\" \"-output:{outputXml}\"",
                WorkingDirectory = targetFolder,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            //Console.WriteLine($"{info.FileName} {info.Arguments}");

            var process = Process.Start(info);
            Assert.NotNull(process);
            var console = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            Assert.True(File.Exists(outputXml));

            _scenarioContext["OutputXml"] = outputXml;
        }

        [Then(@"I should have a results\.xml file with a coverage greater than or equal to '(.*)'%")]
        public void ThenIShouldHaveAResults_XmlFileWithACoverageGreaterThanOrEqualTo(int coveragePercentage)
        {
            var xml = File.ReadAllText((string) _scenarioContext["OutputXml"]);
            var coverage = Utils.GetTotalCoverage(xml) ?? "-1";
            Assert.GreaterOrEqual(decimal.Parse(coverage), coveragePercentage);
        }
    }
}
