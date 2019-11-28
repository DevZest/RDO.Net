using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class AttributeNode : SymbolNode
        {
            public static AttributeNode CreateCustomValidator(ModelMapper mapper, (INamedTypeSymbol, AttributeData) info)
            {
                return Create(mapper, info, NodeKind.CustomValidator);
            }

            public static AttributeNode CreateUniqueConstraint(ModelMapper mapper, (INamedTypeSymbol, AttributeData) info)
            {
                return Create(mapper, info, NodeKind.UniqueConstraint);
            }

            public static AttributeNode CreateIndex(ModelMapper mapper, (INamedTypeSymbol, AttributeData) info)
            {
                return Create(mapper, info, NodeKind.Index);
            }

            public static AttributeNode CreateCheckConstraint(ModelMapper mapper, (INamedTypeSymbol, AttributeData) info)
            {
                return Create(mapper, info, NodeKind.CheckConstraint);
            }

            public static AttributeNode CreateComputation(ModelMapper mapper, (INamedTypeSymbol, AttributeData) info)
            {
                return Create(mapper, info, NodeKind.Computation);
            }

            private static AttributeNode Create(ModelMapper mapper, (INamedTypeSymbol Type, AttributeData Attribute) info, NodeKind kind)
            {
                var type = info.Type;
                var attribute = info.Attribute;
                var symbol = GetImplementationSymbol(type, attribute, mapper.Compilation);
                return symbol == null ? null : new AttributeNode(mapper, attribute, symbol, kind);
            }

            private static ISymbol GetImplementationSymbol(INamedTypeSymbol type, AttributeData attribute, Compilation compilation)
            {
                return attribute.GetImplementationSymbol(type, GetName(attribute), compilation);
            }

            private AttributeNode(ModelMapper mapper, AttributeData attribute, ISymbol symbol, NodeKind kind)
                : base(mapper, symbol)
            {
                AttributeData = attribute;
                _kind = kind;
            }

            public AttributeData AttributeData { get; }

            private static string GetName(AttributeData namedModelAttribute)
            {
                return namedModelAttribute.GetStringArgument();
            }

            private readonly NodeKind _kind;
            public override NodeKind Kind
            {
                get { return _kind; }
            }
        }
    }
}
