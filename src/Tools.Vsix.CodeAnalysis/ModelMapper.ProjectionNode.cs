using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class ProjectionNode : SymbolNodeBase
        {
            public ProjectionNode(ModelMapper modelMapper, INamedTypeSymbol typeSymbol)
                : base(modelMapper)
            {
                TypeSymbol = typeSymbol;
            }

            public INamedTypeSymbol TypeSymbol { get; }

            public override ISymbol Symbol
            {
                get { return TypeSymbol; }
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Projection; }
            }
        }
    }
}
