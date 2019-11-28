using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed class RelationshipImplementationNode : Node
        {
            public RelationshipImplementationNode(DbMapper mapper, IMethodSymbol implementation)
                : base(mapper)
            {
                _implementation = implementation;
            }

            public override NodeKind Kind => NodeKind.RelationshipImplementation;

            private readonly IMethodSymbol _implementation;
            public override ISymbol Symbol => _implementation;
        }
    }
}
