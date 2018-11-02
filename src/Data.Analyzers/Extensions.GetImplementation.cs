using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static (bool IsProperty, ITypeSymbol ReturnType, ITypeSymbol[] ParameterTypes)? GetImplementation(this ITypeSymbol attributeClass, Compilation compilation)
        {
            var implementationAttribute = attributeClass.GetAttributes().Where(x => x.AttributeClass.EqualsTo(KnownTypes.ImplementationAttribute, compilation)).FirstOrDefault();
            if (implementationAttribute == null)
                return null;

            var constructorArguments = implementationAttribute.ConstructorArguments;
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
