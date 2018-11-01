using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static bool BaseTypeEqualsTo(this ITypeSymbol type, string knownType, Compilation compilation)
        {
            return type.BaseType.EqualsTo(knownType, compilation);
        }
    }
}
