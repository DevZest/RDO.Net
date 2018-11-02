using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static ISymbol GetCrossReferenceSymbol(this AttributeData attribute, INamedTypeSymbol type, string name, Compilation compilation)
        {
            var crossRefAttributeType = GetCrossReferenceAttributeType(attribute, compilation);
            if (crossRefAttributeType == null)
                return null;

            return type.GetMembers(name).Where(x => HasAttribute(x, crossRefAttributeType)).FirstOrDefault();
        }

        public static bool HasAttribute(this ISymbol symbol, ITypeSymbol attributeClass)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass.Equals(attributeClass));
        }
    }
}
