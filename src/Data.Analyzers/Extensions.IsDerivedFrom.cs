using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static bool IsDerivedFrom(this ITypeSymbol type, string knownType, Compilation compilation)
        {
            return type.IsDerivedFrom(compilation.GetKnownType(knownType));
        }

        public static bool IsDerivedFrom(this ITypeSymbol type, INamedTypeSymbol baseType)
        {
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.Equals(baseType) || currentType.OriginalDefinition.Equals(baseType))
                    return true;
            }
            return false;
        }
    }
}
