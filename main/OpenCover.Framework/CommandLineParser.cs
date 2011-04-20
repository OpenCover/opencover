using System;
using System.Text;
using OpenCover.Framework.Common;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.Framework
{
    public enum Architecture
    {
        Arch32 = 32,
        Arch64 = 64
    }

    /// <summary>
    /// Parse the command line arguments and set the appropriate properties
    /// </summary>
    public class CommandLineParser : CommandLineParserBase, ICommandLine
    {
        public CommandLineParser(string arguments)
            : base(arguments)
        {
            Architecture = Architecture.Arch32;
            PortNumber = 0xBABE;
            HostOnlySeconds = 20;
            OutputFile = "results.xml";
            Filters = new List<string>();
        }

        public string Usage()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("    -target:<target application>");
            builder.AppendLine("    [-targetdir:<target directory>]");
            builder.AppendLine("    [-targetargs:<arguments for the target process>]");
            builder.AppendLine("    [-port:<port number>]");
            builder.AppendLine("    [-register[:user]]");
            builder.AppendLine("    [-arch:<32|64>]");
            builder.AppendLine("    [-output:<path to file>]");
            builder.AppendLine("    [-type:Sequence,Method,Branch]");
            builder.AppendLine("    [-filter:<space seperated filters> ]");
            builder.AppendLine("    [-nodefaultfilters]");
            builder.AppendLine("or");
            builder.AppendLine("    -host:<time in seconds>");
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
            return builder.ToString();
        }

        public void ExtractAndValidateArguments()
        {
            foreach (var key in ParsedArguments.Keys)
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
                    case "filter":
                        Filters = GetArgumentValue("filter").Split(" ".ToCharArray()).ToList();
                        break;
                    case "type":
                        var types = GetArgumentValue("type").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        CoverageType = CoverageType.None;
                        foreach (var type in types)
                        {
                            CoverageType coverageType;
                            if (!CoverageType.TryParse(type, true, out coverageType))
                            {
                                throw new InvalidOperationException(string.Format("The type {0} is not recognised", type));
                            }
                            CoverageType |= coverageType;
                        }
                        break;
                    case "port":
                        int port = 0;
                        if (int.TryParse(GetArgumentValue("port"), out port))
                        {
                            PortNumber = port;
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("The port argument did not have a valid portnumber i.e. -port:8000 {0}", GetArgumentValue("port")));
                        }
                        break;
                    case "host":
                        HostOnly = true;
                        var hostValue = GetArgumentValue("host");
                        if (hostValue == string.Empty) break;
                        int time = 0;
                        if (int.TryParse(hostValue, out time))
                        {
                            HostOnlySeconds = time;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                "The port argument did not have a valid portnumber i.e. -port:8000");
                        }
                        break;
                    case "arch":
                        Architecture arch;
                        var val = GetArgumentValue("arch");
                        if (!Architecture.TryParse(val, true, out arch))
                        {
                            throw new InvalidOperationException(string.Format("The arch {0} is not recognised", val));
                        }
                        if (arch != Framework.Architecture.Arch32 && arch != Framework.Architecture.Arch64)
                        {
                            throw new InvalidOperationException(string.Format("The arch {0} was not recognised", val));
                        }
                        Architecture = arch;
                        break;
                    case "?":
                        PrintUsage = true;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("The argument {0} is not recognised", key));
                }
            }

            if (HostOnly || PrintUsage) return;

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
        /// the switch -port with the argument of a port number to use i.e. -port:8000
        /// </summary>
        public int PortNumber { get; private set; }

        /// <summary>
        /// Run in host only mode i.e. no coverage -host
        /// </summary>
        /// <remarks>
        /// Used during development to extract contract data i.e. wsdl and xsds
        /// </remarks>
        public bool HostOnly { get; private set; }

        /// <summary>
        /// How long to run in host only mode, default is 20 seconds
        /// </summary>
        public int HostOnlySeconds { get; private set; }

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
        /// The runtime architecture to register the profiler for
        /// </summary>
        public Architecture Architecture { get; private set; }

        /// <summary>
        /// The name of the output file
        /// </summary>
        public string OutputFile { get; private set; }

        /// <summary>
        /// What type of coverage is required, can be a combination
        /// </summary>
        public CoverageType CoverageType { get; private set; }

        /// <summary>
        /// If specified then the default filters should not be applied
        /// </summary>
        public bool NoDefaultFilters { get; private set; }

        /// <summary>
        /// A list of filters
        /// </summary>
        public List<string> Filters { get; private set; }
    }

}