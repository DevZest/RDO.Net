using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static bool EqualsTo(this ITypeSymbol type, string knownType, Compilation compilation)
        {
            var x = compilation.GetKnownType(knownType);
            return x.Equals(type) || x.Equals(type.OriginalDefinition);
        }
    }
}
