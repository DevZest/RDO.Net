using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static (bool IsProperty, INamedTypeSymbol ReturnType, INamedTypeSymbol[] ParameterTypes)? GetImplementation(this ITypeSymbol attributeClass, Compilation compilation)
        {
            var implementationAttribute = attributeClass.GetAttributes().Where(x => x.AttributeClass.EqualsTo(KnownTypes.ImplementationAttribute, compilation)).FirstOrDefault();
            if (implementationAttribute == null)
                return null;

            var constructorArguments = implementationAttribute.ConstructorArguments;
            var isProperty = (bool)constructorArguments[0].Value;
            var returnType = (INamedTypeSymbol)constructorArguments[1].Value;
            var parameterTypes = (INamedTypeSymbol[])constructorArguments[2].Value;

            return (isProperty, returnType, parameterTypes);
        }
    }
}
