using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static (bool IsProperty, ITypeSymbol ReturnType, ITypeSymbol[] ParameterTypes)? GetModelDeclarationSpec(this ITypeSymbol attributeClass, Compilation compilation)
        {
            Debug.Assert(attributeClass.IsDerivedFrom(KnownTypes.ModelDeclarationAttribute, compilation));

            var specAttribute = attributeClass.GetAttributes().Where(x => x.AttributeClass.EqualsTo(KnownTypes.ModelDeclarationSpecAttribute, compilation)).FirstOrDefault();
            if (specAttribute == null)
                return null;

            var constructorArguments = specAttribute.ConstructorArguments;

            var isProperty = (bool)constructorArguments[0].Value;

            var returnType = (ITypeSymbol)constructorArguments[1].Value;

            var values = constructorArguments[2].Values;
            var parameterTypes = new ITypeSymbol[values.Length];
            for (int i = 0; i < values.Length; i++)
                parameterTypes[i] = (ITypeSymbol)values[i].Value;

            return (isProperty, returnType, parameterTypes);
        }
    }
}
