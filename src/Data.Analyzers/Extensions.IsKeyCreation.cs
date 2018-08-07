using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsKeyCreation(this IMethodSymbol symbol, Compilation compilation)
        {
            if (symbol == null)
                return false;

            var overriddenMethod = symbol.OverriddenMethod;
            if (overriddenMethod == null)
                return false;

            var attributes = overriddenMethod.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => x.AttributeClass.IsTypeOfCreateKeyAttribute(compilation));
        }

        private static bool IsTypeOfCreateKeyAttribute(this ITypeSymbol type, Compilation compilation)
        {
            return type.Equals(compilation.GetTypeByMetadataName("DevZest.Data.Annotations.Primitives.CreateKeyAttribute"));
        }
    }
}
