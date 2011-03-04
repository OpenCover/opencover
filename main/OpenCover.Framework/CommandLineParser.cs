using System;

namespace OpenCover.Framework
{
    /// <summary>
    /// Parse the command line arguments and set the appropriate properties
    /// </summary>
    public class CommandLineParser : CommandLineParserBase
    {
        public CommandLineParser(string arguments) : base(arguments)
        {
            PortNumber = 0xBABE;
            ValidateArguments();
        }

        protected void ValidateArguments()
        {
            foreach (var key in ParsedArguments.Keys)
            {
                switch(key.ToLowerInvariant())
                {
                    case "register":
                        Register = true;
                        UserRegistration = (GetArgumentValue("register").ToLowerInvariant() == "user");
                        break;
                    case "port":
                        int port = 0;
                        PortNumber = int.TryParse(GetArgumentValue("port"), out port) ? port : 0xBABE;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("The argument {0} is not recognised", key));
                }
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
    }
}