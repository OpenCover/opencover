using System;
using System.Text.RegularExpressions;

namespace OpenCover.Framework.Filtering
{
    internal class RegexFilter
    {
        private readonly Lazy<Regex> regex;

        internal string FilterExpression { get; private set; }

        public RegexFilter(string filterExpression, bool shouldWrapExpression = true)
        {
            FilterExpression = filterExpression;
            if (shouldWrapExpression)
            {
                filterExpression = filterExpression.WrapWithAnchors();
            }

            regex = new Lazy<Regex>(() => new Regex(filterExpression));
        }

        public bool IsMatchingExpression(string input)
        {
            return regex.Value.IsMatch(input);
        }
    }
}
