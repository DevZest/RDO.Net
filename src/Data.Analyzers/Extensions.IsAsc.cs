using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsAsc(this AttributeData attributeData, Compilation compilation)
        {
            return attributeData.AttributeClass.Equals(compilation.TypeOfAscAttribute());
        }

        private static INamedTypeSymbol TypeOfAscAttribute(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Annotations.AscAttribute");
        }
    }
}
