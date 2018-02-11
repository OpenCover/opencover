//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace OpenCover.MSBuild
{

    /// <summary>Executes the OpenCover tool with the specified arguments.</summary>
    /// <example>
    /// <code><![CDATA[
    /// <Target Name="Test">
    ///   <OpenCover
    ///     Target="%(NUnitConsole.Identity)"
    ///     TargetArgs="OpenCover.Test.dll /noshadow"
    ///     Filter="+[Open*]*;-[OpenCover.T*]*"
    ///     Output="opencovertests.xml"
    ///   />
    /// </Target>
    /// ]]></code>
    /// </example>
    public class OpenCover:
        ToolTask
    {

        /// <summary>
        /// Creates a new instance of the <see cref="OpenCover"/> task.
        /// </summary>
        public OpenCover()
        {
            DefaultFilters=true;
            Register=true;
        }

        /// <summary>
        /// Returns the  path to the OpenCover tool.
        /// </summary>
        /// <returns>The full path to the OpenCover tool.</returns>
        protected override string GenerateFullPathToTool()
        {
            string exe=Path.GetFileName(ToolExe);

            if (!string.IsNullOrEmpty(ToolPath))
                return Path.GetFullPath(Path.Combine(ToolPath, exe));

            // NuGet deployment
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(OpenCover)).Location), "..\\tools", exe);
            if (File.Exists(path))
                return Path.GetFullPath(path);

            // OpenCover has been installed
            RegistryKey key=null;

            string[] keyNames=new string[] { _OpenCoverRegKey, _OpenCoverRegKeyWow6432 };
            foreach (string kn in keyNames)
            {
                key=Registry.CurrentUser.OpenSubKey(kn);
                if (key!=null)
                    break;

                key=Registry.LocalMachine.OpenSubKey(kn);
                if (key!=null)
                    break;
            }

            if (key==null)
            {
                Log.LogError("Could not find OpenCover installation registry key. Please install OpenCover or repair installation.");
                return null;
            }

            path=(string)key.GetValue(_OpenCoverRegValue);
            if (string.IsNullOrEmpty(path))
            {
                Log.LogError("Could not find OpenCover installation path. Please repair OpenCover installation.");
                return null;
            }

            return Path.GetFullPath(Path.Combine(path, exe));
        }

        /// <summary>
        /// Generates the command line arguments for the OpenCover tool.
        /// </summary>
        /// <returns>The command line arguments for the OpenCover tool.</returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder=new CommandLineBuilder();

            if (Service)
                builder.AppendSwitch("-service");
            if (Register)
                builder.AppendSwitch("-register:user");
            if (!DefaultFilters)
                builder.AppendSwitch("-nodefaultfilters");
            if (MergeByHash)
                builder.AppendSwitch("-mergebyhash");
            if (SkipAutoProps)
                builder.AppendSwitch("-skipautoprops");
            if (ShowUnvisited)
                builder.AppendSwitch("-showunvisited");
            if (ReturnTargetCode)
                builder.AppendSwitch("-returntargetcode" + (TargetCodeOffset != 0 ? string.Format(CultureInfo.InvariantCulture, ":{0}", TargetCodeOffset) : null));

            builder.AppendSwitchIfNotNull("-target:", Target);
            builder.AppendSwitchIfNotNull("-targetdir:", TargetWorkingDir);
            builder.AppendSwitchIfNotNull("-targetargs:", TargetArgs);

            if ((Filter!=null) && (Filter.Length>0))
                builder.AppendSwitchIfNotNull("-filter:", string.Join<ITaskItem>(" ", Filter));

            if ((ExcludeByAttribute!=null) && (ExcludeByAttribute.Length>0))
                builder.AppendSwitchIfNotNull("-excludebyattribute:", string.Join<ITaskItem>(";", ExcludeByAttribute));

            if ((ExcludeByFile!=null) && (ExcludeByFile.Length>0))
                builder.AppendSwitchIfNotNull("-excludebyfile:", string.Join<ITaskItem>(";", ExcludeByFile));

            if ((CoverByTest!=null) && (CoverByTest.Length>0))
                builder.AppendSwitchIfNotNull("-coverbytest:", string.Join<ITaskItem>(";", CoverByTest));

            if ((HideSkipped != null) && (HideSkipped.Length > 0))
                builder.AppendSwitchIfNotNull("-hideskipped:", string.Join<ITaskItem>(";", HideSkipped));

            builder.AppendSwitchIfNotNull("-output:", Output);

            return builder.ToString();
        }

        /// <summary>
        /// Gets the working directory for the OpenCover tool.
        /// </summary>
        /// <returns>The working directory for the OpenCover tool.</returns>
        protected override string GetWorkingDirectory()
        {
            string ret=null;
            if (TargetWorkingDir!=null)
                ret=TargetWorkingDir.GetMetadata("FullPath");

            if (string.IsNullOrEmpty(ret))
                ret=base.GetWorkingDirectory();

            return ret;
        }

        /// <summary>
        /// Logs the OpenCover output.
        /// </summary>
        /// <param name="singleLine">A single line output by the OpenCover tool.</param>
        /// <param name="messageImportance">The importance of the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(singleLine, MessageImportance.Normal);
        }

        /// <summary>
        /// Indicates whether default filters should be applied or not.
        /// </summary>
        public bool DefaultFilters
        {
            get;
            set;
        }

        /// <summary>
        /// Gather coverage by test.
        /// </summary>
        public ITaskItem[] CoverByTest
        {
            get;
            set;
        }

        /// <summary>
        /// Exclude a class or method by filters that match attributes.
        /// </summary>
        public ITaskItem[] ExcludeByAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Exclude a class or method by filters that match filenames.
        /// </summary>

        public ITaskItem[] ExcludeByFile
        {
            get;
            set;
        }

        /// <summary>
        /// A list of filters to apply.
        /// </summary>
        public ITaskItem[] Filter
        {
            get;
            set;
        }

        /// <summary>
        /// Merge the result by assembly file-hash.
        /// </summary>
        public bool MergeByHash
        {
            get;
            set;
        }

        /// <summary>
        /// Remove information from output file.
        /// HideSkipped = <criteria>[;<criteria>]*
        /// <criteria> = [File|Filter|Attribute|MissingPdb|All
        /// </summary>
        public ITaskItem[] HideSkipped
        {
            get;
            set;
        }

        /// <summary>
        /// Neither track nor record auto-implemented properties.
        /// That is, skip getters and setters like these:
        /// public bool Service { get; set; }
        /// </summary>
        public bool SkipAutoProps
        {
            get;
            set;
        }

        /// <summary>
        /// The location and name of the output XML file.
        /// </summary>
        public ITaskItem Output
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the code coverage profiler should be registered or not.
        /// </summary>
        public bool Register
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the list of unvisited methods and classes should be shown.
        /// </summary>
        public bool ShowUnvisited
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether target is a service rather than a regular executable.
        /// </summary>
        public bool Service
        {
            get;
            set;
        }

        /// <summary>
        /// The target application.
        /// </summary>
        [Required]
        public ITaskItem Target
        {
            get;
            set;
        }

        /// <summary>
        /// The working directory for the target process.
        /// </summary>
        public ITaskItem TargetWorkingDir
        {
            get;
            set;
        }

        /// <summary>
        /// Arguments to be passed to the target process.
        /// </summary>
        public string TargetArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Return the target process return code instead of the OpenCover console return code.
        /// </summary>
        public bool ReturnTargetCode
        {
            get;
            set;
        }

        /// <summary>
        /// Use the offset to return the OpenCover console at a value outside the range returned by the target process.
        /// Valid only if ReturnTargetCode is set.
        /// </summary>
        public int TargetCodeOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the OpenCover tool executable.
        /// </summary>
        protected override string ToolName
        {
            get
            {
                return "OpenCover.Console.exe";
            }
        }

        private const string _OpenCoverRegKey=@"SOFTWARE\OpenCover\";
        private const string _OpenCoverRegKeyWow6432=@"SOFTWARE\Wow6432Node\OpenCover\";
        private const string _OpenCoverRegValue="Path";
    }
}
