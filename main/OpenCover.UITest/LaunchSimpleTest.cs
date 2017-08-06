using System.Diagnostics;
using System.IO;
//using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace OpenCover.UITest
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    //[CodedUITest]
    public class LaunchSimpleTest
    {
        public LaunchSimpleTest()
        {
        }

        [TestMethod]
        [DeploymentItem(@".\exe\OpenCover.Simple.Target.exe", "exe")]
        [DeploymentItem(@".\exe\OpenCover.Simple.Target.exe.config", "exe")]
        [DeploymentItem(@".\exe\OpenCover.Simple.Target.pdb", "exe")]
        public void RunApp()
        {
            var c = Directory.GetCurrentDirectory();
            var path = Path.Combine(c, @".\exe\OpenCover.Simple.Target.exe");
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            var pi = new ProcessStartInfo(path);
            pi.EnvironmentVariables["Stuff"] = "1";
            pi.UseShellExecute = false;
            //pi.LoadUserProfile = true;
            //var application = ApplicationUnderTest.Launch(pi);
            //application.Process.WaitForExit(10000);
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;
    }
}
