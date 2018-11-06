using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensionis
    {
        public static AttributeTargets GetAttributeTargets(this AttributeData attributeData, Compilation compilation)
        {
            var attributeClass = attributeData.AttributeClass;
            return attributeClass == null ? AttributeTargets.All : GetAttributeTargets(attributeData.AttributeClass, compilation.GetKnownType(KnownTypes.AttributeUsageAttribute));
        }

        public static AttributeTargets GetAttributeTargets(this INamedTypeSymbol attributeClass, Compilation compilation)
        {
            return GetAttributeTargets(attributeClass, compilation.GetKnownType(KnownTypes.AttributeUsageAttribute));
        }

        private static AttributeTargets GetAttributeTargets(this INamedTypeSymbol attributeClass, INamedTypeSymbol attributeUsageType)
        {
            Debug.Assert(attributeClass != null);

            var attributeUsage = attributeClass.GetAttributes().Where(x => attributeUsageType.Equals(x.AttributeClass)).FirstOrDefault();
            var result = GetAttributeTargets(attributeUsage);
            if (result.HasValue)
                return result.Value;

            var baseType = attributeClass.BaseType;
            return baseType == null ? AttributeTargets.All : GetAttributeTargets(baseType, attributeUsageType);
        }

        private static AttributeTargets? GetAttributeTargets(AttributeData attributeUsage)
        {
            if (attributeUsage == null)
                return null;

            var constructorArguments = attributeUsage.ConstructorArguments;
            if (constructorArguments.Length == 0)
                return null;

            if (constructorArguments[0].Value is int result)
                return (AttributeTargets)result;
            else
                return null;
        }
    }
}
