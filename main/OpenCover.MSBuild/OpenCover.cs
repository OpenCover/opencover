//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace OpenCover.MSBuild
{

    /// <summary>Executes OpenCover with the specified arguments.</summary>
    /// <example>
    /// <code><![CDATA[
    /// <Target Name="Test">
    ///   <OpenCover
    ///     Target="xunit.console.exe"
    ///     TargetArgs="&quot;C:\My tests folder\MyTests.xunit&quot; /silent"
    ///   />
    /// </Target>
    /// ]]></code>
    /// </example>
    public class OpenCover:
        ToolTask
    {

        public OpenCover()
        {
            DefaultFilters=true;
            Register=true;
        }

        protected override string GenerateFullPathToTool()
        {
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
                Log.LogError("Could not find OpenCover installation registry key");
                return null;
            }

            string rd=(string)key.GetValue(_OpenCoverRegValue);
            if (string.IsNullOrEmpty(rd))
            {
                Log.LogError("Could not find OpenCover installation path");
                return null;
            }

            return Path.Combine(rd, ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder=new CommandLineBuilder();

            if (Register)
                builder.AppendSwitch("-register:user");
            if (!DefaultFilters)
                builder.AppendSwitch("-nodefaultfilters");
            if (MergeByHash)
                builder.AppendSwitch("-mergebyhash");
            if (ShowUnvisited)
                builder.AppendSwitch("-showunvisited");

            builder.AppendSwitchIfNotNull("-target:", Target);
            builder.AppendSwitchIfNotNull("-targetdir:", TargetWorkingDir);
            builder.AppendSwitchIfNotNull("-targetargs", TargetArgs);

            var filters=new List<ITaskItem>();
            if (Include!=null)
            {
                foreach (ITaskItem ti in Include)
                    ti.ItemSpec=string.Concat('+', ti.ItemSpec);
                filters.AddRange(Include);
            }
            if (Exclude!=null)
            {
                foreach (ITaskItem ti in Exclude)
                    ti.ItemSpec=string.Concat('-', ti.ItemSpec);
                filters.AddRange(Exclude);
            }
            if (filters.Count>0)
                builder.AppendSwitchIfNotNull("-filters:", filters.ToArray<ITaskItem>(), " ");

            builder.AppendSwitchIfNotNull("-output:", Output);

            return builder.ToString();
        }

        protected override string GetWorkingDirectory()
        {
            string ret=null;
            if (TargetWorkingDir!=null)
                ret=TargetWorkingDir.GetMetadata("FullPath");

            if (string.IsNullOrEmpty(ret))
                ret=base.GetWorkingDirectory();

            return ret;
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (_OutputRegex.IsMatch(singleLine))
            {
                base.LogEventsFromTextOutput(singleLine, MessageImportance.Low);
                _ToolStarted=true;
                return;
            }


            base.LogEventsFromTextOutput(singleLine, (_ToolStarted ? MessageImportance.Normal : MessageImportance.Low));
        }

        public bool DefaultFilters
        {
            get;
            set;
        }

        public ITaskItem[] Include
        {
            get;
            set;
        }

        public ITaskItem[] Exclude
        {
            get;
            set;
        }

        public bool MergeByHash
        {
            get;
            set;
        }

        public ITaskItem Output
        {
            get;
            set;
        }

        public bool Register
        {
            get;
            set;
        }

        public bool ShowUnvisited
        {
            get;
            set;
        }

        [Required]
        public ITaskItem Target
        {
            get;
            set;
        }

        public ITaskItem TargetWorkingDir
        {
            get;
            set;
        }

        public string TargetArgs
        {
            get;
            set;
        }

        protected override string ToolName
        {
            get
            {
                return "OpenCover.Console.exe";
            }
        }

        private bool _ToolStarted;

        private static Regex _OutputRegex=new Regex(@"^\[\d{5}\] \[\d{5}\] ", RegexOptions.Compiled | RegexOptions.Multiline);
        private const string _OpenCoverRegKey=@"SOFTWARE\OpenCover\";
        private const string _OpenCoverRegKeyWow6432=@"SOFTWARE\Wow6432Node\OpenCover\";
        private const string _OpenCoverRegValue="Path";
    }
}
