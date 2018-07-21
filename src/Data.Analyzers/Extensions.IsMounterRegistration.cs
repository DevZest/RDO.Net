using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsMounterRegistration(this IMethodSymbol symbol, Compilation compilation)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => x.AttributeClass.IsTypeOfMounterRegistrationAttribute(compilation));
        }

        private static bool IsTypeOfMounterRegistrationAttribute(this ITypeSymbol type, Compilation compilation)
        {
            return type.Equals(compilation.GetTypeByMetadataName("DevZest.Data.Annotations.Primitives.MounterRegistrationAttribute"));
        }
    }
}
