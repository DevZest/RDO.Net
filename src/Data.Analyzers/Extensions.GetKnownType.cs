using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetKnownType(this Compilation compilation, string knownType)
        {
            return compilation.GetTypeByMetadataName(knownType);
        }
    }
}
