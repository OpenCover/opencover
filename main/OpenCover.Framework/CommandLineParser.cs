using System;

namespace OpenCover.Framework
{
    public class CommandLineParser : CommandLineParserBase
    {
        public CommandLineParser(string arguments) : base(arguments)
        {
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
                    default:
                        throw new InvalidOperationException(string.Format("The argument {0} is not recognised", key));
                }
            }
        }

        public bool Register { get; private set; }
        public bool UserRegistration { get; private set; }
    }
}