using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetAddonTypeKey(this INamedTypeSymbol type, Compilation compilation)
        {
            var addonAttribute = type.GetAttribute(compilation.GetKnownType(KnownTypes.AddonAttribute));
            if (addonAttribute == null)
                return null;

            var arguments = addonAttribute.ConstructorArguments;
            if (arguments.Length == 0)
                return null;

            var argument = arguments[0];
            if (argument.Kind != TypedConstantKind.Type)
                return null;

            return argument.Value as INamedTypeSymbol;
        }
    }
}
