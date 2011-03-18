using System;
using System.Text;

namespace OpenCover.Framework
{
    /// <summary>
    /// Parse the command line arguments and set the appropriate properties
    /// </summary>
    public class CommandLineParser : CommandLineParserBase, ICommandLine
    {
        public CommandLineParser(string arguments) : base(arguments)
        {
            PortNumber = 0xBABE;
            HostOnlySeconds = 20;
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
            builder.AppendLine("or");
            builder.AppendLine("    -host:<time in seconds>");
            builder.AppendLine("or");
            builder.AppendLine("    -?");
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
                    case "port":
                        int port = 0;
                        if (int.TryParse(GetArgumentValue("port"), out port))
                        {
                            PortNumber = port;
                        }
                        else
                        {
                            throw new InvalidOperationException("The port argument did not have a valid portnumber i.e. -port:8000");
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

        public bool PrintUsage { get; private set; }
    }

}