using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class ChildModelNode : SymbolNode
        {
            public ChildModelNode(ModelMapper modelMapper, IPropertySymbol propertySymbol)
                : base(modelMapper, propertySymbol)
            {
            }

            public override NodeKind Kind
            {
                get { return NodeKind.ChildModel; }
            }
        }
    }
}
