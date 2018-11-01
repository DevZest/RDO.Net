using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static bool IsDerivedFrom(this ITypeSymbol type, string knownType, Compilation compilation)
        {
            var x = compilation.GetKnownType(knownType);
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(x))
                    return true;
            }
            return false;
        }
    }
}
