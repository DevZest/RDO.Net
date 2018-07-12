using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class Extensions
    {
        public static bool IsTypeOfLocalColumn(this ITypeSymbol type, Compilation compilation)
        {
            return type.OriginalDefinition.Equals(compilation.GetTypeByMetadataName("DevZest.Data.LocalColumn`1"));
        }
    }
}
