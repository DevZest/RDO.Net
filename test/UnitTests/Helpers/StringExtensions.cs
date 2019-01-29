using System.Text.RegularExpressions;

namespace DevZest.Data.MySql.Helpers
{
    internal static class StringExtensions
    {
        internal static string RemoveGuids(this string value)
        {
            string pattern = @"([a-z0-9]{8}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{12})";
            var matches = Regex.Matches(value, pattern);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                value = value.Replace(match.Value, string.Empty);
            }
            return value;
        }
    }
}
