using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace OpenCover.Specs.Steps
{
    [Binding]
    public class DotNetCoreSteps
    {
        [Given(@"I can find the OpenCover application")]
        public void GivenICanFindTheOpenCoverApplication()
        {
#if DEBUG
            var targetFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\bin\Debug"));
#else
            var targetFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\bin\Release"));
#endif
            Assert.IsTrue(File.Exists(Path.Combine(targetFolder, "OpenCover.Console.exe")));

            ScenarioContext.Current["TargetFolder"] = targetFolder;
        }

        [Given(@"I can find the target application")]
        public void GivenICanFindTheTargetApplication()
        {
#if DEBUG
            var targetApp = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\OpenCover.Simple.Target.Core\bin\Debug\netcoreapp1.0\OpenCover.Simple.Target.Core.dll"));
#else
            var targetApp = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DotNetCoreSteps).Assembly.Location) ?? ".", @"..\..\..\OpenCover.Simple.Target.Core\bin\Release\netcoreapp1.0\OpenCover.Simple.Target.Core.dll"));
#endif
            Assert.IsTrue(File.Exists(targetApp));

            ScenarioContext.Current["TargetApp"] = targetApp;
        }

        [When(@"I execute OpenCover against the target application using the oldstyle switch")]
        public void WhenIExecuteOpenCoverAgainstTheTargetApplicationUsingTheOldstyleSwitch()
        {
            var dotnetexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\dotnet.exe");
            var targetApp = (string)ScenarioContext.Current["TargetApp"];
            var targetFolder = (string)ScenarioContext.Current["TargetFolder"];
            var outputXml = Path.Combine(Path.GetDirectoryName(targetApp) ?? ".", "results.xml");
            if (File.Exists(outputXml))
                File.Delete(outputXml);

            var info = new ProcessStartInfo
            {
                FileName = Path.Combine(targetFolder, "OpenCover.Console.exe"),
                Arguments = $"-oldstyle -register:user \"-target:{dotnetexe}\" \"-targetargs:{targetApp}\" \"-output:{outputXml}\"",
                WorkingDirectory = targetFolder,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = Process.Start(info);
            Assert.NotNull(process);
            var console = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Assert.True(File.Exists(outputXml));

            ScenarioContext.Current["OutputXml"] = outputXml;
        }
        
        [Then(@"I should have a '(.*)' file with a coverage greater than or equal to '(.*)'%")]
        public void ThenIShouldHaveAFileWithACoverageGreaterThan(string resultsFile, decimal coveragePercentage)
        {
            var xml = File.ReadAllText((string) ScenarioContext.Current["OutputXml"]);
            var coverage = Utils.GetTotalCoverage(xml);
            Assert.GreaterOrEqual(decimal.Parse(coverage), coveragePercentage);
        }
    }
}
