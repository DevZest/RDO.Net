using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static INamedTypeSymbol TypeOfPrimaryKey(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.PrimaryKey");
        }
    }
}
