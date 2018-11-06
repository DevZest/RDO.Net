using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static (ITypeSymbol[] AddonTypes, ITypeSymbol[] ValidOnTypes)? GetModelMemberAttributeSpec(this AttributeData attribute, Compilation compilation)
        {
            return attribute.AttributeClass.GetModelMemberAttributeSpec(compilation);
        }

        public static (INamedTypeSymbol[] AddonTypes, INamedTypeSymbol[] ValidOnTypes)? GetModelMemberAttributeSpec(this INamedTypeSymbol attributeClass, Compilation compilation)
        {
            var specAttribute = attributeClass.GetAttribute(compilation.GetKnownType(KnownTypes.ModelMemberAttributeSpecAttribute));
            if (specAttribute == null)
                return null;

            var constructorArguments = specAttribute.ConstructorArguments;

            var addonTypes = GetSpecParame(constructorArguments, 0);
            var validOnTypes = GetSpecParame(constructorArguments, 1);

            return (addonTypes, validOnTypes);

            INamedTypeSymbol[] GetSpecParame(ImmutableArray<TypedConstant> constructorParameters, int index)
            {
                if (index >= constructorParameters.Length)
                    return null;

                var parameter = constructorParameters[index];
                if (parameter.Kind != TypedConstantKind.Array)
                    return null;

                var values = parameter.Values;
                var result = new INamedTypeSymbol[values.Length];
                for (int i = 0; i < values.Length; i++)
                    result[i] = values[i].Value as INamedTypeSymbol;

                return result;
            }
        }
    }
}
