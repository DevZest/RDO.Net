using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static ImmutableArray<IParameterSymbol> GetKeyConstructorParams(this IMethodSymbol methodSymbol, Compilation compilation, out bool isKeyCreation)
        {
            isKeyCreation = IsKeyCreation(methodSymbol, compilation);
            if (!isKeyCreation)
                return ImmutableArray<IParameterSymbol>.Empty;

            if (!(methodSymbol.ReturnType is INamedTypeSymbol keyType))
                return ImmutableArray<IParameterSymbol>.Empty;

            var constructor = keyType.GetSingleConstructor();
            return constructor == null ? ImmutableArray<IParameterSymbol>.Empty : constructor.Parameters;
        }

        private static bool IsKeyCreation(this IMethodSymbol methodSymbol, Compilation compilation)
        {
            if (methodSymbol == null)
                return false;

            var overriddenMethod = methodSymbol.OverriddenMethod;
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
