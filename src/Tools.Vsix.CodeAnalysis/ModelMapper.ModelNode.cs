using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class ModelNode : SymbolNodeBase
        {
            public ModelNode(ModelMapper modelMapper)
                : base(modelMapper)
            {
            }

            public override ISymbol Symbol
            {
                get { return Mapper.ModelType; }
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Model; }
            }

            public override INavigatableMarker CreateMarker(bool navigationSuggested)
            {
                return null;
            }
        }
    }
}
