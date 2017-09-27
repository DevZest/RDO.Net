namespace DevZest.Data.Utilities
{
    internal static partial class StringExtensions
    {
        internal static string LastPart(this string str, char separator)
        {
            var lastIndex = str.LastIndexOf(separator);
            return lastIndex < 0 ? str : str.Substring(lastIndex + 1);
        }
    }
}
