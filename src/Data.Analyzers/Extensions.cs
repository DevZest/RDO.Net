using Microsoft.CodeAnalysis;

namespace DevZest.Data.Analyzers
{
    internal static class Extensions
    {
        public static T SelfOrFirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            return (node is T result) ? result : node.FirstAncestor<T>();
        }

        public static T FirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            for (var current = node.Parent; current != null; current = current.Parent)
            {
                if (current is T result)
                    return result;
            }

            return null;
        }

        public static bool IsTypeOfMounterRegistrationAttribute(this ITypeSymbol type, Compilation compilation)
        {
            return type.Equals(compilation.GetTypeByMetadataName("DevZest.Data.Annotations.Primitives.MounterRegistrationAttribute"));
        }

        public static bool IsTypeOfColumn(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Column").IsBaseTypeOf(type);
        }

        public static bool IsTypeOfLocalColumn(this ITypeSymbol type, Compilation compilation)
        {
            return type.OriginalDefinition.Equals(compilation.GetTypeByMetadataName("DevZest.Data.LocalColumn`1"));
        }

        public static bool IsTypeOfProjection(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Projection").IsBaseTypeOf(type);
        }

        public static bool IsTypeOfColumnList(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.ColumnList").IsBaseTypeOf(type);
        }

        public static bool IsTypeOfModel(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Model").IsBaseTypeOf(type);
        }

        public static bool IsTypeOfMounter(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Mounter`1").Equals(type.OriginalDefinition);
        }

        private static bool IsBaseTypeOf(this INamedTypeSymbol @this, ITypeSymbol type)
        {
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(@this))
                    return true;
            }
            return false;
        }

        public static INamedTypeSymbol GetPrimaryKeyType(this INamedTypeSymbol type, Compilation compilation)
        {
            var genericModelType = compilation.GetTypeByMetadataName("DevZest.Data.Model`1");

            INamedTypeSymbol baseModelType = null;
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(genericModelType))
                {
                    baseModelType = currentType;
                    break;
                }
            }

            return baseModelType.TypeArguments[0] as INamedTypeSymbol;
        }
    }
}
