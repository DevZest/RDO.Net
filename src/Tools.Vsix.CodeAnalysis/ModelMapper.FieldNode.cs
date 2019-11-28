using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class FieldNode : SymbolNodeBase
        {
            public FieldNode(ModelMapper modelMapper, IPropertySymbol propertySymbol)
                : base(modelMapper)
            {
                Debug.Assert(IsField(propertySymbol, modelMapper.Compilation));
                _propertySymbol = propertySymbol;
            }

            private readonly IPropertySymbol _propertySymbol;
            public override ISymbol Symbol
            {
                get { return _propertySymbol; }
            }

            public override NodeKind Kind
            {
                get { return GetFieldKind(_propertySymbol, Compilation).Value; }
            }

            public static bool IsField(IPropertySymbol propertySymbol, Compilation compilation)
            {
                return propertySymbol.SetMethod != null && GetFieldKind(propertySymbol, compilation).HasValue;
            }

            private static NodeKind? GetFieldKind(IPropertySymbol propertySymbol, Compilation compilation)
            {
                Debug.Assert(propertySymbol != null);

                var registrationType = propertySymbol.GetPropertyRegistrationType(compilation);

                if (registrationType == PropertyRegistrationType.LocalColumn)
                    return NodeKind.LocalColumn;
                else if (registrationType == PropertyRegistrationType.ColumnList)
                    return NodeKind.ColumnList;
                else if (registrationType == PropertyRegistrationType.ModelColumn || registrationType == PropertyRegistrationType.ProjectionColumn)
                    return NodeKind.Column;
                else if (registrationType == PropertyRegistrationType.Projection)
                    return NodeKind.Projection;
                else
                    return null;
            }
        }
    }
}
