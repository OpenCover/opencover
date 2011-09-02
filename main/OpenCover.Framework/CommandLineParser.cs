//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Text;
using OpenCover.Framework.Common;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.Framework
{

    /// <summary>
    /// Parse the command line arguments and set the appropriate properties
    /// </summary>
    public class CommandLineParser : CommandLineParserBase, ICommandLine
    {
        public CommandLineParser(string[] arguments)
            : base(arguments)
        {
            OutputFile = "results.xml";
            Filters = new List<string>();
        }

        public string Usage()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("    -target:<target application>");
            builder.AppendLine("    [-targetdir:<target directory>]");
            builder.AppendLine("    [-targetargs:[\"]<arguments for the target process>[\"]]");
            builder.AppendLine("    [-register[:user]]");
            builder.AppendLine("    [-output:[\"]<path to file>[\"]]");
            builder.AppendLine("    [-filter:[\"]<space seperated filters>[\"]]");
            builder.AppendLine("    [-nodefaultfilters]");
            builder.AppendLine("    [-mergebyhash]");
            builder.AppendLine("    [-showunvisited]");
            builder.AppendLine("    [-returntargetcode]");
            builder.AppendLine("or");
            builder.AppendLine("    -?");
            builder.AppendLine("");
            builder.AppendLine("Filters:");
            builder.AppendLine("    Filters are used to include and exclude assemblies and types in the");
            builder.AppendLine("    profiler coverage. Two default exclude filters are always applied to");
            builder.AppendLine("    exclude the System.* and Microsoft.* assemblies unless the");
            builder.AppendLine("    -nodefaultfilters option is supplied. If no other filters are supplied");
            builder.AppendLine("    via the -filter option then a default inclusive all filter +[*]* is");
            builder.AppendLine("    applied.");
            builder.AppendLine("Notes:");
            builder.AppendLine("    Enclose arguments in quotes \"\" when spaces are required see -targetargs.");

            return builder.ToString();
        }

        public void ExtractAndValidateArguments()
        {
            foreach (string key in ParsedArguments.Keys)
            {
                switch(key.ToLowerInvariant())
                {
                    case "register":
                        Register = true;
                        UserRegistration = (GetArgumentValue("register").ToLowerInvariant() == "user");
                        break;
                    case "target":
                        Target = GetArgumentValue("target");
                        break;
                    case "targetdir":
                        TargetDir = GetArgumentValue("targetdir");
                        break;
                    case "targetargs":
                        TargetArgs = GetArgumentValue("targetargs");
                        break;
                    case "output":
                        OutputFile = GetArgumentValue("output");
                        break;
                    case "nodefaultfilters":
                        NoDefaultFilters = true;
                        break;
                    case "mergebyhash":
                        MergeByHash = true;
                        break;
                    case "showunvisited":
                        ShowUnvisited = true;
                        break;
                    case "returntargetcode":
                        ReturnTargetCode = true;
                        break;
                    case "filter":
                        Filters = GetArgumentValue("filter").Split(" ".ToCharArray()).ToList();
                        break;
                    case "?":
                        PrintUsage = true;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("The argument {0} is not recognised", key));
                }
            }

            if (PrintUsage) return;

            if (string.IsNullOrWhiteSpace(Target))
            {
                throw new InvalidOperationException("The target argument is required");
            }
        }

        /// <summary>
        /// the switch -register was supplied
        /// </summary>
        public bool Register { get; private set; }

        /// <summary>
        /// the switch -register with the user argument was supplied i.e. -register:user
        /// </summary>
        public bool UserRegistration { get; private set; }

        /// <summary>
        /// The target executable that is to be profiles
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// The working directory that the action is to take place
        /// </summary>
        public string TargetDir { get; private set; }

        /// <summary>
        /// The arguments that are to be passed to the Target
        /// </summary>
        public string TargetArgs { get; private set; }

        /// <summary>
        /// Requests that the user wants to see the commandline help
        /// </summary>
        public bool PrintUsage { get; private set; }

        /// <summary>
        /// The name of the output file
        /// </summary>
        public string OutputFile { get; private set; }

        /// <summary>
        /// If specified then the default filters should not be applied
        /// </summary>
        public bool NoDefaultFilters { get; private set; }

        /// <summary>
        /// If specified then results to be merged by matching hash 
        /// </summary>
        public bool MergeByHash { get; private set; }

        /// <summary>
        /// Show the unvisited classes/methods at the end of the coverage run
        /// </summary>
        public bool ShowUnvisited { get; private set; }

        /// <summary>
        /// Show the unvisited classes/methods at the end of the coverage run
        /// </summary>
        public bool ReturnTargetCode { get; private set; }

        /// <summary>
        /// A list of filters
        /// </summary>
        public List<string> Filters { get; private set; }
    }

}