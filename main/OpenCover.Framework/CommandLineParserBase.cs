//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.Framework
{
    /// <summary>
    /// Parse the command line arguments based on the following syntax: <br/>
    /// [-argument[:optional-value]] [-argument[:optional-value]]
    /// </summary>
    public abstract class CommandLineParserBase
    {
        private readonly string[] _arguments;

        /// <summary>
        /// Instantiate the base command line parser
        /// </summary>
        /// <param name="arguments"></param>
        protected CommandLineParserBase(string[] arguments)
        {
            _arguments = arguments;
            ParsedArguments = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get the parsed arguments
        /// </summary>
        protected IDictionary<string, string> ParsedArguments { get; private set; }
        
        /// <summary>
        /// Parse the arguments
        /// </summary>
        protected void ParseArguments()
        {
            if (_arguments == null) 
                return;
            if (ParsedArguments.Count > 0) 
                return;

            foreach (var argument in _arguments)
            {
                string trimmed;
                if (ExtractTrimmedArgument(argument, out trimmed)) 
                    continue;

                ExtractArgumentValue(trimmed);
            }
        }

        private void ExtractArgumentValue(string trimmed)
        {
            var colonidx = trimmed.IndexOf(':');
            if (colonidx > 0)
            {
                var key = trimmed.Substring(0, colonidx).ToLowerInvariant();
                var val = trimmed.Substring(colonidx + 1);
                if (!ParsedArguments.ContainsKey(key))
                    ParsedArguments.Add(key, val);
                else
                    ParsedArguments[key] = (ParsedArguments[key] + " " + val).Trim();
            }
            else
            {
                trimmed = trimmed.ToLowerInvariant();
                if (!ParsedArguments.ContainsKey(trimmed))
                    ParsedArguments.Add(trimmed, String.Empty);
            }
        }

        private static bool ExtractTrimmedArgument(string argument, out string trimmed)
        {
            trimmed = argument.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return true;

            if (!trimmed.StartsWith("-"))
                throw new InvalidOperationException(string.Format("The argument '{0}' is not recognised", argument));

            trimmed = trimmed.Substring(1);
            return string.IsNullOrEmpty(trimmed);
        }

        /// <summary>
        /// Get the number of extracted arguments
        /// </summary>
        public int ArgumentCount { get { return ParsedArguments.Count; } }

        /// <summary>
        /// Check if an argument of the name given was part of the supplied arguments
        /// </summary>
        /// <param name="argument">an argument name</param>
        /// <returns>true - if argument was supplied</returns>
        public bool HasArgument(string argument) 
        {
            return ParsedArguments.ContainsKey(argument); 
        }

        /// <summary>
        /// Get the the value of a named argument
        /// </summary>
        /// <param name="argument">an argument name</param>
        /// <returns>the value supplied by an argument</returns>
        public string GetArgumentValue(string argument)
        {
            return HasArgument(argument) ? ParsedArguments[argument] : String.Empty;
        }
        
    }
}
