using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class KeyNode : SymbolNode
        {
            public KeyNode(ModelMapper modelMapper, INamedTypeSymbol typeSymbol)
                : base(modelMapper, typeSymbol)
            {
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Key; }
            }
        }
    }
}
