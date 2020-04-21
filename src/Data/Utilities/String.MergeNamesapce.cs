using System.Diagnostics;

namespace DevZest
{
    internal static partial class Extensions
    {
        internal static string GetFullName(this string name, string @namespace)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            return string.IsNullOrEmpty(@namespace) ? name : $"{@namespace}.{name}";
        }
    }
}
