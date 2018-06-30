using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    internal static class Extensions
    {
        public static bool IsMounterRegistration(this IMethodSymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => TypeIdentifier.MounterRegistrationAttribute.IsSameTypeOf(x.AttributeClass));
        }

        public static ModelMemberKind? GetModelMemberKind(this IPropertySymbol property)
        {
            if (!(property.Type is INamedTypeSymbol propertyType))
                return null;

            var parent = property.GetModelMemberParent();
            if (!parent.HasValue)
                return null;

            var parentValue = parent.Value;

            if (TypeIdentifier.Column.IsBaseTypeOf(propertyType))
                return parentValue == ModelMemberParent.Model ? ModelMemberKind.ModelColumn : ModelMemberKind.ColumnGroupMember;
            else if (TypeIdentifier.LocalColumn.IsBaseTypeOf(propertyType))
                return ModelMemberKind.LocalColumn;
            else if (TypeIdentifier.ColumnGroup.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ColumnGroup;
            else if (TypeIdentifier.ColumnList.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ColumnList;
            else if (TypeIdentifier.Model.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ChildModel;
            else
                return null;
        }

        private enum ModelMemberParent
        {
            Model,
            ColumnGroup
        }

        private static ModelMemberParent? GetModelMemberParent(this IPropertySymbol property)
        {
            var containingType = property.ContainingType;
            if (TypeIdentifier.Model.IsBaseTypeOf(containingType))
                return ModelMemberParent.Model;
            else if (TypeIdentifier.ColumnGroup.IsBaseTypeOf(containingType))
                return ModelMemberParent.ColumnGroup;
            else
                return null;
        }
    }
}
