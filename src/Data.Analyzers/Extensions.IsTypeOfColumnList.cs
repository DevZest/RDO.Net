using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsTypeOfColumnList(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.ColumnList").IsBaseTypeOf(type);
        }
    }
}
