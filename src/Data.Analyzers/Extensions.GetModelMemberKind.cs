using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        private static bool IsTypeOfMounterRegistrationAttribute(this ITypeSymbol type, Compilation compilation)
        {
            return type.Equals(compilation.GetTypeByMetadataName("DevZest.Data.Annotations.Primitives.MounterRegistrationAttribute"));
        }

        private static bool IsTypeOfColumn(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Column").IsBaseTypeOf(type);
        }

        private static bool IsTypeOfProjection(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Projection").IsBaseTypeOf(type);
        }

        private static bool IsTypeOfColumnList(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.ColumnList").IsBaseTypeOf(type);
        }

        private static bool IsTypeOfModel(this ITypeSymbol type, Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("DevZest.Data.Model").IsBaseTypeOf(type);
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

        public static bool IsMounterRegistration(this IMethodSymbol symbol, Compilation compilation)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => x.AttributeClass.IsTypeOfMounterRegistrationAttribute(compilation));
        }

        public static ModelMemberKind? GetModelMemberKind(this IPropertySymbol property, Compilation compilation)
        {
            if (!(property.Type is INamedTypeSymbol propertyType))
                return null;

            var parent = property.GetModelMemberParent(compilation);
            if (!parent.HasValue)
                return null;

            var parentValue = parent.Value;

            if (propertyType.IsTypeOfLocalColumn(compilation))
                return ModelMemberKind.LocalColumn;
            else if (propertyType.IsTypeOfColumn(compilation))
                return parentValue == ModelMemberParent.Model ? ModelMemberKind.ModelColumn : ModelMemberKind.ProjectionColumn;
            else if (propertyType.IsTypeOfProjection(compilation))
                return ModelMemberKind.Projection;
            else if (propertyType.IsTypeOfColumnList(compilation))
                return ModelMemberKind.ColumnList;
            else if (propertyType.IsTypeOfModel(compilation))
                return ModelMemberKind.ChildModel;
            else
                return null;
        }

        private enum ModelMemberParent
        {
            Model,
            Projection
        }

        private static ModelMemberParent? GetModelMemberParent(this IPropertySymbol property, Compilation compilation)
        {
            var containingType = property.ContainingType;
            if (containingType.IsTypeOfModel(compilation))
                return ModelMemberParent.Model;
            else if (containingType.IsTypeOfProjection(compilation))
                return ModelMemberParent.Projection;
            else
                return null;
        }
    }
}
