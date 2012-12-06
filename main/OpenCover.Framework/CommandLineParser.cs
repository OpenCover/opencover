//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCover.Framework.Model;
using log4net.Core;

namespace OpenCover.Framework
{

    /// <summary>
    /// Parse the command line arguments and set the appropriate properties
    /// </summary>
    public class CommandLineParser : CommandLineParserBase, ICommandLine
    {
        /// <summary>
        /// Constructs the parser
        /// </summary>
        /// <param name="arguments">An array of command line arguments</param>
        public CommandLineParser(string[] arguments)
            : base(arguments)
        {
            OutputFile = "results.xml";
            Filters = new List<string>();
            AttributeExclusionFilters = new List<string>();
            FileExclusionFilters = new List<string>();
            TestFilters = new List<string>();
            LogLevel = Level.Info;
            HideSkipped = new List<SkippedMethod>();
        }

        /// <summary>
        /// Get the usage string 
        /// </summary>
        /// <returns>The usage string</returns>
        public string Usage()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("    [\"]-target:<target application>[\"]");
            builder.AppendLine("    [[\"]-targetdir:<target directory>[\"]]");
            builder.AppendLine("    [[\"]-targetargs:<arguments for the target process>[\"]]");
            builder.AppendLine("    [-register[:user]]");
            builder.AppendLine("    [[\"]-output:<path to file>[\"]]");
            builder.AppendLine("    [[\"]-filter:<space separated filters>[\"]]");
            builder.AppendLine("    [[\"]-filterfile:<path to file>[\"]]");
            builder.AppendLine("    [-nodefaultfilters]");
            builder.AppendLine("    [-mergebyhash]");
            builder.AppendLine("    [-showunvisited]");
            builder.AppendLine("    [-returntargetcode[:<opencoverreturncodeoffset>]]");
            builder.AppendLine("    [-excludebyattribute:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [-excludebyfile:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [-coverbytest:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [-hideskipped:File|Filter|Attribute|MissingPdb|All,[File|Filter|Attribute|MissingPdb|All]]");
            builder.AppendLine("    [-log:[Off|Fatal|Error|Warn|Info|Debug|Verbose|All]]");
            builder.AppendLine("    [-service]");
            builder.AppendLine("    [-oldStyle]");
            builder.AppendLine("or");
            builder.AppendLine("    -?");
            builder.AppendLine("");
            builder.AppendLine("For further information on the command line please visit the wiki");
            builder.AppendLine("    https://github.com/sawilde/opencover/wiki/Usage");
            builder.AppendLine("");
            builder.AppendLine("Filters:");
            builder.AppendLine("    Filters are used to include and exclude assemblies and types in the");
            builder.AppendLine("    profiler coverage; see the Usage guide. If no other filters are supplied");
            builder.AppendLine("    via the -filter option then a default inclusive all filter +[*]* is");
            builder.AppendLine("    applied.");
            builder.AppendLine("Logging:");
            builder.AppendLine("    Logging is based on log4net logging levels and appenders - defaulting");
            builder.AppendLine("    to a ColouredConsoleAppender and INFO log level.");
            builder.AppendLine("Notes:");
            builder.AppendLine("    Enclose arguments in quotes \"\" when spaces are required see -targetargs.");

            return builder.ToString();
        }

        /// <summary>
        /// Extract the arguments and validate them; also validate the supplied options when simple
        /// </summary>
        public void ExtractAndValidateArguments()
        {
            foreach (var key in ParsedArguments.Keys)
            {
                var lower = key.ToLowerInvariant();
                switch(lower)
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
                        var argument = GetArgumentValue("returntargetcode");
                        if (argument != string.Empty)
                        {
                            int offset;
                            if (int.TryParse(argument, out offset))
                                ReturnCodeOffset = offset;
                            else
                                throw new InvalidOperationException("The return target code offset must be an integer");
                        }
                        break;
                    case "filter":
                        Filters = ExtractFilters(GetArgumentValue("filter"));
                        break;
                    case "filterfile":
                        FilterFile = GetArgumentValue("filterfile");
                        break;
                    case "excludebyattribute":
                        AttributeExclusionFilters = GetArgumentValue("excludebyattribute")
                            .Split(';').ToList();
                        break;
                    case "excludebyfile":
                        FileExclusionFilters = GetArgumentValue("excludebyfile")
                            .Split(';').ToList();
                        break;
                    case "hideskipped":
                        HideSkipped = ExtractSkipped(GetArgumentValue("hideskipped"));
                        break;
                    case "coverbytest":
                        TestFilters = GetArgumentValue("coverbytest")
                            .Split(';').ToList();
                        break;
                    case "log":
                        var value = GetArgumentValue("log");
                        LogLevel = (Level)typeof(Level).GetFields(BindingFlags.Static | BindingFlags.Public)
                            .First(x => string.Compare(x.Name, value, true, CultureInfo.InvariantCulture) == 0).GetValue(typeof(Level));
                        break;
                    case "service":
                        Service = true;
                        break;
                    case "oldstyle":
                        OldStyleInstrumentation = true;
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

        private static List<string> ExtractFilters(string rawFilters)
        {
            const string strRegex = @"([+-][\[].*?[\]].+?\s)|([+-][\[].*?[\]].*)";
            const RegexOptions myRegexOptions = RegexOptions.None;
            var myRegex = new Regex(strRegex, myRegexOptions);
            
            return (from Match myMatch in myRegex.Matches(rawFilters) where myMatch.Success select myMatch.Value.Trim()).ToList();
        }

        private static List<SkippedMethod> ExtractSkipped(string skipped)
        {
            if (string.IsNullOrWhiteSpace(skipped)) skipped = "All";
            var options = skipped.Split(';');
            var list = new List<SkippedMethod>();
            foreach (var option in options)
            {
                switch (option.ToLowerInvariant())
                {
                    case "all":
                        list.Add(SkippedMethod.Attribute);
                        list.Add(SkippedMethod.File);
                        list.Add(SkippedMethod.Filter);
                        list.Add(SkippedMethod.MissingPdb);
                        break;
                    default:
                        SkippedMethod result;
                        if (!Enum.TryParse(option, true, out result))
                        {
                            throw new InvalidOperationException(string.Format("The hideskipped option {0} is not valid", option));
                        }
                        list.Add(result);
                        break;
                }
            }
            return list.Distinct().ToList();
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

        /// <summary>
        /// A File that has additional filters, one per line.
        /// </summary>
        public string FilterFile { get; private set; }

        /// <summary>
        /// The offset for the return code - this is to help avoid collisions between opencover return codes and the target
        /// </summary>
        public int ReturnCodeOffset { get; private set; }

        /// <summary>
        /// A list of attribute exclusion filters
        /// </summary>
        public List<string> AttributeExclusionFilters { get; private set; }
    
        /// <summary>
        /// A list of file exclusion filters
        /// </summary>
        public List<string> FileExclusionFilters { get; private set; }

        /// <summary>
        /// A list of test file filters
        /// </summary>
        public List<string> TestFilters { get; private set; }

        /// <summary>
        /// A list of skipped entities to hide from being ouputted
        /// </summary>
        public List<SkippedMethod> HideSkipped { get; private set; }

        /// <summary>
        /// The logging level based on log4net.Core.Level
        /// </summary>
        public Level LogLevel { get; private set; }

        /// <summary>
        /// This switch means we should treat the mandatory target as a service
        /// </summary>
        public bool Service { get; private set; }

        /// <summary>
        /// Use the old style of instrumentation that even though not APTCA friendly will
        /// work when - ngen install /Profile "mscorlib" - has been used
        /// </summary>
        public bool OldStyleInstrumentation { get; private set; }
    }

}