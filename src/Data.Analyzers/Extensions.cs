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

        public static INamedTypeSymbol TypeOfMounterRegistrationAttribute(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Annotations.Primitives.MounterRegistrationAttribute");
        }

        public static INamedTypeSymbol TypeOfColumn(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Column");
        }

        public static INamedTypeSymbol TypeOfLocalColumn(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.LocalColumn`1");
        }

        public static INamedTypeSymbol TypeOfProjection(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Projection");
        }

        public static INamedTypeSymbol TypeOfColumnList(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.ColumnList");
        }

        public static INamedTypeSymbol TypeOfModel(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Model");
        }

        public static INamedTypeSymbol TypeOfGenericModel(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Model`1");
        }

        public static INamedTypeSymbol TypeOfMounter(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Mounter`1");
        }

        public static bool IsBaseTypeOf(this INamedTypeSymbol @this, ITypeSymbol type)
        {
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(@this))
                    return true;
            }
            return false;
        }
    }
}
