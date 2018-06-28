using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    internal static class Extensions
    {
        public static bool IsMounterRegistration(this IMethodSymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => TypeIdentifier.MounterRegistrationAttribute.IsSameTypeOf(x.AttributeClass));
        }

        public static bool IsMountable(this INamedTypeSymbol type)
        {
            var attributes = type.GetAttributes();
            if (attributes == null)
                return false;
            if (attributes.Any(x => TypeIdentifier.MounterRegistrationAttribute.IsSameTypeOf(x.AttributeClass)))
                return true;

            var baseType = type.BaseType;
            return baseType == null ? false : IsMountable(baseType);
        }

        public static bool IsMountable(this IPropertySymbol property)
        {
            return (property.Type is INamedTypeSymbol type) ? TypeIdentifier.ModelMember.IsBaseTypeOf(type) : false;
        }
    }
}
