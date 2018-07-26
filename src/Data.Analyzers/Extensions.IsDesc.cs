using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsDesc(this AttributeData attributeData, Compilation compilation)
        {
            return attributeData.AttributeClass.Equals(compilation.TypeOfDescAttribute());
        }

        private static INamedTypeSymbol TypeOfDescAttribute(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Annotations.DescAttribute");
        }
    }
}
