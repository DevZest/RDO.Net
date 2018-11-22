using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static string FormatString(this ITypeSymbol[] parameterTypes)
        {
            return string.Join(", ", (object[])parameterTypes);
        }
    }
}
