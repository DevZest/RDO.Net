using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetModelType(this IPropertySymbol dbTable)
        {
            return (dbTable.Type is INamedTypeSymbol dbTableType) ? dbTableType.TypeArguments[0] as INamedTypeSymbol : null;
        }
    }
}
