using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class PrimaryKeyNode : SymbolNodeBase
        {
            public PrimaryKeyNode(ModelMapper mapper)
                : base(mapper)
            {
            }

            public INamedTypeSymbol PkType
            {
                get { return Mapper.PkType; }
            }

            public sealed override ISymbol Symbol
            {
                get { return PkType; }
            }

            public override NodeKind Kind
            {
                get { return NodeKind.PrimaryKey; }
            }
        }
    }
}
