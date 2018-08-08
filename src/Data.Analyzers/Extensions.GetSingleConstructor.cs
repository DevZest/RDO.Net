using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static IMethodSymbol GetSingleConstructor(this INamedTypeSymbol classSymbol)
        {
            if (classSymbol.Constructors.Length != 1)
                return null;

            var result = classSymbol.Constructors[0];
            return result.IsStatic || result.IsImplicitlyDeclared ? null : result;
        }
    }
}
