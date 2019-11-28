using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed class RelationshipDeclarationNode : Node
        {
            public RelationshipDeclarationNode(DbMapper mapper, IPropertySymbol table, AttributeData attributeData, string name)
                : base(mapper)
            {
                _table = table;
                _attributeData = attributeData;
                _name = name;
            }

            public override NodeKind Kind => NodeKind.RelationshipDeclaration;

            private readonly IPropertySymbol _table;
            public override ISymbol Symbol => _table;

            private readonly AttributeData _attributeData;
            protected override AttributeData AttributeData => _attributeData;

            private readonly string _name;
            public override string Name => _name;

            public override Location Location => AttributeData.GetLocation();

            public override Location LocalLocation
            {
                get
                {
                    var result = Location;
                    return result.SourceTree == SyntaxTree ? result : null;
                }
            }
        }
    }
}
