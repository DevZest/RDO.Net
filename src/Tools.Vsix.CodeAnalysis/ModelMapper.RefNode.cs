using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class RefNode : SymbolNode
        {
            public RefNode(ModelMapper modelMapper, INamedTypeSymbol typeSymbol)
                : base(modelMapper, typeSymbol)
            {
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Ref; }
            }
        }
    }
}
