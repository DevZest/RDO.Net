using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsTypeOfProjection(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Projection").IsBaseTypeOf(type);
        }
    }
}
