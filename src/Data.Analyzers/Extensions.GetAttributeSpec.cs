using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static (INamedTypeSymbol[] AddonTypes, INamedTypeSymbol[] ValidOnTypes, bool RequiresArgument)? GetAttributeSpec(this AttributeData attribute, Compilation compilation)
        {
            return attribute.AttributeClass.GetAttributeSpec(compilation);
        }

        public static (INamedTypeSymbol[] AddonTypes, INamedTypeSymbol[] ValidOnTypes, bool RequiresArgument)? GetAttributeSpec(this INamedTypeSymbol attributeClass, Compilation compilation)
        {
            var specAttribute = attributeClass.GetAttribute(compilation.GetKnownType(KnownTypes.AttributeSpecAttribute));
            if (specAttribute == null)
                return null;

            var constructorArguments = specAttribute.ConstructorArguments;

            var addonTypes = GetSpecArgument(constructorArguments, 0);
            var validOnTypes = GetSpecArgument(constructorArguments, 1);
            var requiresArgument = GetRequiresArgument(specAttribute.NamedArguments);

            return (addonTypes, validOnTypes, requiresArgument);
        }

        private static INamedTypeSymbol[] GetSpecArgument(ImmutableArray<TypedConstant> constructorArguments, int index)
        {
            if (index >= constructorArguments.Length)
                return null;

            var argument = constructorArguments[index];
            if (argument.Kind != TypedConstantKind.Array)
                return null;

            var values = argument.Values;
            if (values == null)
                return null;

            var result = new INamedTypeSymbol[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = values[i].Value as INamedTypeSymbol;

            return result;
        }

        private static bool GetRequiresArgument(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
        {
            for (int i = 0; i < namedArguments.Length; i++)
            {
                var namedArguemnt = namedArguments[i];
                if (namedArguemnt.Key == "RequiresArgument")
                {
                    var argument = namedArguemnt.Value;
                    if (argument.Value is bool result)
                        return result;
                }
            }
            return false;
        }
    }
}
