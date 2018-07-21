using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsBaseTypeOf(this INamedTypeSymbol @this, ITypeSymbol type)
        {
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(@this))
                    return true;
            }
            return false;
        }
    }
}
