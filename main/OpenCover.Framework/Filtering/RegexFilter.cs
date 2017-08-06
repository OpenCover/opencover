using System;
using System.Text.RegularExpressions;

namespace OpenCover.Framework.Filtering
{
    internal class RegexFilter
    {
        private readonly Lazy<Regex> _regex;

        internal string FilterExpression { get; private set; }

        public RegexFilter(string filterExpression, bool shouldWrapExpression = true)
        {
            FilterExpression = filterExpression;
            _regex = new Lazy<Regex>(() => new Regex(shouldWrapExpression ? filterExpression.WrapWithAnchors() : filterExpression));
        }

        public bool IsMatchingExpression(string input)
        {
            return _regex.Value.IsMatch(input);
        }
    }
}
