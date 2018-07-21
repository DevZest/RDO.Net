using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
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
