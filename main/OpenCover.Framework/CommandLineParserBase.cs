//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCover.Framework
{
    /// <summary>
    /// Parse the command line arguments based on the following syntax: <br/>
    /// [-argument[:optional-value]] [-argument[:optional-value]]
    /// </summary>
    public abstract class CommandLineParserBase
    {
        private readonly string[] _arguments;

        protected CommandLineParserBase(string[] arguments)
        {
            _arguments = arguments;
            ParsedArguments = new Dictionary<string, string>();
            ParseArguments();
        }

        protected IDictionary<string, string> ParsedArguments { get; private set; }

        private void ParseArguments()
        {
            if (_arguments == null) return;

            foreach (var argument in _arguments)
            {
                var trimmed = argument.Trim();
                if (!trimmed.StartsWith("-")) continue;
                trimmed = trimmed.Substring(1);
                if (string.IsNullOrEmpty(trimmed)) continue;
                var colonidx = trimmed.IndexOf(':');
                if (colonidx>0)
                {
                    var arg = trimmed.Substring(0, colonidx);
                    var val = trimmed.Substring(colonidx + 1);
                    if (!ParsedArguments.ContainsKey(arg))
                        ParsedArguments.Add(arg, val);
                    else
                        ParsedArguments[arg] = ParsedArguments[arg] + " " + val; 
                }
                else
                {
                    if (!ParsedArguments.ContainsKey(trimmed))
                        ParsedArguments.Add(trimmed, String.Empty);
                }       
            }
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
