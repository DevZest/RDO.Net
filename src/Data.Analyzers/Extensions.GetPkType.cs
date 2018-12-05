using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetPkType(this INamedTypeSymbol type, Compilation compilation)
        {
            return type.GetArgumentType(KnownTypes.GenericModel, compilation);
        }
    }
}
