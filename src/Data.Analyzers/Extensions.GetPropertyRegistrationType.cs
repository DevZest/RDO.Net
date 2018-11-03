using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static PropertyRegistrationType? GetPropertyRegistrationType(this IPropertySymbol property, Compilation compilation)
        {
            if (!(property.Type is INamedTypeSymbol propertyType))
                return null;

            var parent = property.GetPropertyRegistrationParentType(compilation);
            if (!parent.HasValue)
                return null;

            var parentValue = parent.Value;

            if (propertyType.EqualsTo(KnownTypes.LocalColumn, compilation))
                return PropertyRegistrationType.LocalColumn;
            else if (propertyType.IsDerivedFrom(KnownTypes.Column, compilation))
                return parentValue == PropertyRegistrationParentType.Model ? PropertyRegistrationType.ModelColumn : PropertyRegistrationType.ProjectionColumn;
            else if (propertyType.IsDerivedFrom(KnownTypes.Projection, compilation))
                return PropertyRegistrationType.Projection;
            else if (propertyType.IsDerivedFrom(KnownTypes.ColumnList, compilation))
                return PropertyRegistrationType.ColumnList;
            else if (propertyType.IsDerivedFrom(KnownTypes.Model, compilation))
                return PropertyRegistrationType.ChildModel;
            else
                return null;
        }

        private enum PropertyRegistrationParentType
        {
            Model,
            Projection
        }

        private static PropertyRegistrationParentType? GetPropertyRegistrationParentType(this IPropertySymbol property, Compilation compilation)
        {
            var containingType = property.ContainingType;
            if (containingType.IsDerivedFrom(KnownTypes.Model, compilation))
                return PropertyRegistrationParentType.Model;
            else if (containingType.IsDerivedFrom(KnownTypes.Projection, compilation))
                return PropertyRegistrationParentType.Projection;
            else
                return null;
        }
    }
}
