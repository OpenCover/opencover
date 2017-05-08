//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCover.Framework.Model;
using log4net.Core;

namespace OpenCover.Framework
{
    /// <summary>
    /// What registration method
    /// </summary>
    public enum Registration
    {
        /// <summary>
        /// normal
        /// </summary>
        Normal,

        /// <summary>
        /// user
        /// </summary>
        User,

        /// <summary>
        /// use path to 32 bit profiler
        /// </summary>
        Path32,

        /// <summary>
        /// use path to 64 bit profiler
        /// </summary>
        Path64
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ServiceEnvironment
    {
        /// <summary>
        /// Default behaviour
        /// </summary>
        None,

        /// <summary>
        /// Service name, not service account
        /// </summary>
        ByName
    }

    /// <summary>
    /// SafeMode values
    /// </summary>
    public enum SafeMode
    {
        /// <summary>
        /// SafeMode is on (default)
        /// </summary>
        On,

        /// <summary>
        /// SafeMode is on (default)
        /// </summary>
        Yes = On,

        /// <summary>
        /// SafeMode is off
        /// </summary>
        Off,

        /// <summary>
        /// SafeMode is off
        /// </summary>
        No = Off
    }

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
            EnablePerformanceCounters = false;
            TraceByTest = false;
            ServiceEnvironment = ServiceEnvironment.None;
            ServiceStartTimeout = new TimeSpan(0, 0, 30);
            RegExFilters = false;
            Registration = Registration.Normal;
            PrintVersion = false;
            ExcludeDirs = new string[0];
            SafeMode = true;
            DiagMode = false;
            SendVisitPointsTimerInterval = 0;
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
            builder.AppendLine("    [[\"]-searchdirs:<additional PDB directory>[;<additional PDB directory>][;<additional PDB directory>][\"]]");
            builder.AppendLine("    [[\"]-targetargs:<arguments for the target process>[\"]]");
            builder.AppendLine("    [-register[:user]]");
            builder.AppendLine("    [[\"]-output:<path to file>[\"]]");
            builder.AppendLine("    [-mergeoutput");
            builder.AppendLine("    [[\"]-filter:<space separated filters>[\"]]");
            builder.AppendLine("    [[\"]-filterfile:<path to file>[\"]]");
            builder.AppendLine("    [-nodefaultfilters]");
            builder.AppendLine("    [-regex]");
            builder.AppendLine("    [-mergebyhash]");
            builder.AppendLine("    [-showunvisited]");
            builder.AppendLine("    [-returntargetcode[:<opencoverreturncodeoffset>]]");
            builder.AppendLine("    [-excludebyattribute:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [-excludebyfile:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [-coverbytest:<filter>[;<filter>][;<filter>]]");
            builder.AppendLine("    [[\"]-excludedirs:<excludedir>[;<excludedir>][;<excludedir>][\"]]");
            var skips = string.Join("|", Enum.GetNames(typeof(SkippedMethod)).Where(x => x != "Unknown"));
            builder.AppendLine(string.Format("    [-hideskipped:{0}|All,[{0}|All]]", skips));
            builder.AppendLine("    [-log:[Off|Fatal|Error|Warn|Info|Debug|Verbose|All]]");
            builder.AppendLine("    [-service[:byname]]");
            builder.AppendLine("    [-servicestarttimeout:<minutes+seconds e.g. 1m23s>");
            builder.AppendLine("    [-communicationtimeout:<integer, e.g. 10000>");
            builder.AppendLine("    [-threshold:<max count>]");
            builder.AppendLine("    [-enableperformancecounters]");
            builder.AppendLine("    [-skipautoprops]");
            builder.AppendLine("    [-oldstyle]");
            builder.AppendLine("    [-safemode:on|off|yes|no]");
            builder.AppendLine("    [-diagmode]");
            builder.AppendLine("    [-sendvisitpointstimerinterval: 0 (no timer) | 1-maxint (timer interval in msec)");
            builder.AppendLine("    -version");
            builder.AppendLine("or");
            builder.AppendLine("    -?");
            builder.AppendLine("or");
            builder.AppendLine("    -version");
            builder.AppendLine("");
            builder.AppendLine("For further information on the command line please visit the wiki");
            builder.AppendLine("    https://github.com/OpenCover/opencover/wiki/Usage");
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
            ParseArguments();

            foreach (var key in ParsedArguments.Keys)
            {
                var lower = key.ToLowerInvariant();
                switch(lower)
                {
                    case "register":
                        Register = true;
                        Registration registration;
                        Enum.TryParse(GetArgumentValue("register"), true, out registration);
                        Registration = registration;
                        break;
                    case "target":
                        Target = GetArgumentValue("target");
                        break;
                    case "targetdir":
                        TargetDir = GetArgumentValue("targetdir");
                        break;
                    case "searchdirs":
                        SearchDirs = GetArgumentValue("searchdirs").Split(';');
                        break;
                    case "excludedirs":
                        ExcludeDirs =
                            GetArgumentValue("excludedirs")
                                .Split(';')
                                .Where(_ => _ != null)
                                .Select(_ => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _)))
                                .Where(Directory.Exists)
                                .Distinct()
                                .ToArray();
                        break;
                    case "targetargs":
                        TargetArgs = GetArgumentValue("targetargs");
                        break;
                    case "output":
                        OutputFile = GetArgumentValue("output");
                        break;
                    case "mergeoutput":
                        MergeExistingOutputFile = true;
                        break;
                    case "nodefaultfilters":
                        NoDefaultFilters = true;
                        break;
                    case "mergebyhash":
                        MergeByHash = true;
                        break;
                    case "regex":
                        RegExFilters = true;
                        break;
                    case "showunvisited":
                        ShowUnvisited = true;
                        break;
                    case "returntargetcode":
                        ReturnTargetCode = true;
                        ReturnCodeOffset = ExtractValue<int>("returntargetcode", () =>
                            { throw new InvalidOperationException("The return target code offset must be an integer"); });
                        break;
                    case "communicationtimeout":
                        CommunicationTimeout = ExtractValue<int>("communicationtimeout", () =>
                        { throw new InvalidOperationException(string.Format("The communication timeout must be an integer: {0}", GetArgumentValue("communicationtimeout"))); });
                        CommunicationTimeout = Math.Max(Math.Min(CommunicationTimeout, 60000), 10000);
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
                        TraceByTest = TestFilters.Any();
                        break;
                    case "log":
                        var value = GetArgumentValue("log");
                        LogLevel = (Level)typeof(Level).GetFields(BindingFlags.Static | BindingFlags.Public)
                            .First(x => string.Compare(x.Name, value, true, CultureInfo.InvariantCulture) == 0).GetValue(typeof(Level));
                        break;
                    case "service":
                        Service = true;
                        ServiceEnvironment val;
                        if (Enum.TryParse(GetArgumentValue("service"), true, out val))
                        {
                            ServiceEnvironment = val;
                        }
                        break;
                    case "servicestarttimeout":
                        var timeoutValue = GetArgumentValue("servicestarttimeout");
                        ServiceStartTimeout = ParseTimeoutValue(timeoutValue);                        
                        break;
                    case "oldstyle":
                        OldStyleInstrumentation = true;
                        break;
                    case "enableperformancecounters":
                        EnablePerformanceCounters = true;
                        break;
                    case "threshold":
                        Threshold = ExtractValue<ulong>("threshold", () =>
                            { throw new InvalidOperationException("The threshold must be an integer"); });
                        break;
                    case "skipautoprops":
                        SkipAutoImplementedProperties = true;
                        break;
                    case "safemode":
                        SafeMode = ExtractSafeMode(GetArgumentValue("safemode")) == Framework.SafeMode.On;
                        break;
                    case "?":
                        PrintUsage = true;
                        break;
                    case "version":
                        PrintVersion = true;
                        break;
                    case "diagmode":
                        DiagMode = true;
                        break;
                    case "sendvisitpointstimerinterval":
                        SendVisitPointsTimerInterval = ExtractValue<uint>("sendvisitpointstimerinterval", () =>
                        { throw new InvalidOperationException("The send visit points timer interval must be a non-negative integer"); });
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("The argument '-{0}' is not recognised", key));
                }
            }

            ValidateArguments();
        }

        private T ExtractValue<T>(string argumentName, Action onError)
        {
            var textValue = GetArgumentValue(argumentName);
            if (!string.IsNullOrEmpty(textValue))
            {
                try
                {
                    return (T)TypeDescriptor
                        .GetConverter(typeof (T))
                        .ConvertFromString(textValue);
                }
                catch (Exception)
                {
                    onError();
                }
            }
            return default(T);
        }

        private static List<string> ExtractFilters(string rawFilters)
        {
            // starts with required +-
            // followed by optional process-filter
            // followed by required assembly-filter 
            // followed by optional class-filter, where class-filter excludes -+" and space characters
            // followed by optional space 
            // NOTE: double-quote character from test-values somehow sneaks into default filter as last character?
            const string strRegex = @"[\-\+](<.*?>)?\[.*?\][^\-\+\s\x22]*";
            const RegexOptions myRegexOptions = RegexOptions.Singleline | RegexOptions.ExplicitCapture;
            var myRegex = new Regex(strRegex, myRegexOptions);
            
            return (from Match myMatch in myRegex.Matches(rawFilters) where myMatch.Success select myMatch.Value.Trim()).ToList();
        }

        private static List<SkippedMethod> ExtractSkipped(string skippedArg)
        {
            var skipped = string.IsNullOrWhiteSpace(skippedArg) ? "All" : skippedArg;
            var options = skipped.Split(';');
            var list = new List<SkippedMethod>();
            foreach (var option in options)
            {
                switch (option.ToLowerInvariant())
                {
                    case "all":
                        list = Enum.GetValues(typeof(SkippedMethod)).Cast<SkippedMethod>().Where(x => x != SkippedMethod.Unknown).ToList();
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

        private static SafeMode ExtractSafeMode(string safeModeArg)
        {
            SafeMode result;
            if (!Enum.TryParse(safeModeArg, true, out result))
            {
                throw new InvalidOperationException(string.Format("The safemode option {0} is not valid", safeModeArg));
            }
            return result;
        }

        private TimeSpan ParseTimeoutValue(string timeoutValue)
        {
            var match = Regex.Match(timeoutValue, @"((?<minutes>\d+)m)?((?<seconds>\d+)s)?");
            if (match.Success)
            {
                int minutes = 0;
                int seconds = 0;

                var minutesMatch = match.Groups["minutes"];
                if (minutesMatch.Success)
                {
                    minutes = int.Parse(minutesMatch.Value);
                }

                var secondsMatch = match.Groups["seconds"];
                if (secondsMatch.Success)
                {
                    seconds = int.Parse(secondsMatch.Value);
                }

                if (minutes == 0 && seconds == 0)
                {
                    throw ExceptionForInvalidArgumentValue(timeoutValue, "servicestarttimeout");
                }

                return new TimeSpan(0, minutes, seconds);
            }
            else
            {
                throw ExceptionForInvalidArgumentValue(timeoutValue, "servicestarttimeout");
            }
        }

        private static Exception ExceptionForInvalidArgumentValue(string argumentName, string argumentValue)
        {
            return new InvalidOperationException(string.Format("Incorrect argument: {0} for {1}", argumentValue, argumentName));
        }

        private void ValidateArguments()
        {
            if (PrintUsage || PrintVersion) 
                return;

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
        /// Set when we should not use thread based buffers. 
        /// May not be as performant in some circumstances but avoids data loss
        /// </summary>
        public bool SafeMode { get; private set; }

        /// <summary>
        /// the switch -register with the user argument was supplied i.e. -register:user
        /// </summary>
        public Registration Registration { get; private set; }

        /// <summary>
        /// whether auto-implemented properties sould be skipped 
        /// </summary>
        public bool SkipAutoImplementedProperties { get; private set; }

        /// <summary>
        /// The target executable that is to be profiled
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// The working directory that the action is to take place
        /// </summary>
        public string TargetDir { get; private set; }

        /// <summary>
        /// Alternate locations where PDBs can be found
        /// </summary>
        public string[] SearchDirs { get; private set; }

        /// <summary>
        /// Assemblies loaded form these dirs will be excluded
        /// </summary>
        public string[] ExcludeDirs { get; private set; }

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
        /// Set the threshold i.e. max visit count reporting
        /// </summary>
        public ulong Threshold { get; private set; }

        /// <summary>
        /// activate trace by test feature
        /// </summary>
        public bool TraceByTest { get; private set; }

        /// <summary>
        /// The logging level based on log4net.Core.Level
        /// </summary>
        public Level LogLevel { get; private set; }

        /// <summary>
        /// This switch means we should treat the mandatory target as a service
        /// </summary>
        public bool Service { get; private set; }

        /// <summary>
        /// Gets the value indicating how to apply the service environment
        /// </summary>
        public ServiceEnvironment ServiceEnvironment { get; private set; }

        /// <summary>
        /// Gets the timeout to wait for the service to start up
        /// </summary>
        public TimeSpan ServiceStartTimeout { get; private set; }

        /// <summary>
        /// Use the old style of instrumentation that even though not APTCA friendly will
        /// work when - ngen install /Profile "mscorlib" - has been used
        /// </summary>
        public bool OldStyleInstrumentation { get; private set; }

        /// <summary>
        /// Enable the performance counters
        /// </summary>
        public bool EnablePerformanceCounters { get; private set; }

        /// <summary>
        /// Filters are to use regular expressions rather than wild cards
        /// </summary>
        public bool RegExFilters { get; private set; }

        /// <summary>
        /// If an existing output exists then load it and allow merging of test runs
        /// </summary>
        public bool MergeExistingOutputFile { get; private set; }

        /// <summary>
        /// Instructs the console to print its version and exit
        /// </summary>
        public bool PrintVersion { get; private set; }

        /// <summary>
        /// Sets the 'short' timeout between profiler and host (normally 10000ms)
        /// </summary>
        public int CommunicationTimeout { get; private set; }

        /// <summary>
        /// Enable diagnostics in the profiler
        /// </summary>
        public bool DiagMode { get; private set; }

        /// <summary>
        /// Enable SendVisitPoints timer interval in msec (0 means do not run timer)
        /// </summary>
        public uint SendVisitPointsTimerInterval { get; private set; }
    }

}