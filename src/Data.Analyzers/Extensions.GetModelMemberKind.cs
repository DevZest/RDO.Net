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

            if (propertyType.EqualsTo(KnownTypes.LocalColumn, compilation))
                return ModelMemberKind.LocalColumn;
            else if (propertyType.IsDerivedFrom(KnownTypes.Column, compilation))
                return parentValue == ModelMemberParent.Model ? ModelMemberKind.ModelColumn : ModelMemberKind.ProjectionColumn;
            else if (propertyType.IsDerivedFrom(KnownTypes.Projection, compilation))
                return ModelMemberKind.Projection;
            else if (propertyType.IsDerivedFrom(KnownTypes.ColumnList, compilation))
                return ModelMemberKind.ColumnList;
            else if (propertyType.IsDerivedFrom(KnownTypes.Model, compilation))
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
            if (containingType.IsDerivedFrom(KnownTypes.Model, compilation))
                return ModelMemberParent.Model;
            else if (containingType.IsDerivedFrom(KnownTypes.Projection, compilation))
                return ModelMemberParent.Projection;
            else
                return null;
        }
    }
}
