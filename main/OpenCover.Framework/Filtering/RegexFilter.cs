using System;
using System.Text.RegularExpressions;

namespace OpenCover.Framework.Filtering
{
    internal class RegexFilter
    {
        private readonly Lazy<Regex> regex;
        internal string FilterExpression { get; private set; }

        public RegexFilter(string filterExpression)
        {
            FilterExpression = filterExpression;
            regex = new Lazy<Regex>(() => new Regex(filterExpression.WrapWithAnchors()));
        }

        public bool IsMatchingExpression(string input)
        {
            return regex.Value.IsMatch(input);
        }
    }
}
