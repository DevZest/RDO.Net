using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static AttributeData GetAttribute(this INamedTypeSymbol type, string knownAttributeType, Compilation compilation)
        {
            return type.GetAttribute(compilation.GetKnownType(knownAttributeType));
        }

        public static AttributeData GetAttribute(this INamedTypeSymbol type, INamedTypeSymbol attributeClass)
        {
            Debug.Assert(attributeClass != null);

            if (type == null)
                return null;

            var result = type.GetAttributes().Where(x => attributeClass.Equals(x.AttributeClass)).FirstOrDefault();
            return result ?? GetAttribute(type.BaseType, attributeClass);
        }
    }
}
