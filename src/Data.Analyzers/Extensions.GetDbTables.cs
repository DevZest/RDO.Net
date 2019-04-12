using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static ImmutableArray<IPropertySymbol> GetDbTables(this INamedTypeSymbol dbType, Compilation compilation)
        {
            var dbTable = compilation.GetKnownType(KnownTypes.DbTableOf);
            return dbType.GetMembers().OfType<IPropertySymbol>().Where(x => dbTable.Equals(x?.Type?.OriginalDefinition)).ToImmutableArray();
        }
    }
}
