using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        public abstract class Node : TreeNode
        {
            protected Node(DbMapper mapper)
            {
                Mapper = mapper;
            }

            public DbMapper Mapper { get; }

            public abstract NodeKind Kind { get; }

            public abstract ISymbol Symbol { get; }

            public override string Name
            {
                get { return Symbol.Name; }
            }

            protected virtual AttributeData AttributeData
            {
                get { return null; }
            }

            protected Compilation Compilation
            {
                get { return Mapper.Compilation; }
            }

            protected SyntaxTree SyntaxTree
            {
                get { return Mapper.SyntaxTree; }
            }

            public override Location Location
            {
                get
                {
                    if (Symbol == null)
                        return null;

                    var locations = Symbol.Locations;
                    return locations.Length > 0 ? locations[0] : null;
                }
            }

            public override Location LocalLocation
            {
                get
                {
                    if (Symbol == null)
                        return null;

                    var locations = Symbol.Locations;
                    for (int i = 0; i < locations.Length; i++)
                    {
                        var location = locations[i];
                        if (location.SourceTree == SyntaxTree)
                            return location;
                    }

                    return null;
                }
            }

            private sealed class Marker : INavigatableMarker
            {
                public Marker(NodeKind nodeKind, ISymbol symbol, string name, bool navigationSuggested)
                    : this(nodeKind, symbol.ContainingType.GetFullyQualifiedMetadataName(), name, navigationSuggested, false)
                {
                }

                public Marker(NodeKind nodeKind, string containingTypeMetadataName, string symbolName, bool navigationSuggested, bool shouldExpand)
                {
                    _nodeKind = nodeKind;
                    _containingTypeMetadataName = containingTypeMetadataName;
                    _name = symbolName;
                    NavigationSuggested = navigationSuggested;
                    ShouldExpand = shouldExpand;
                }

                private readonly NodeKind _nodeKind;
                private readonly string _containingTypeMetadataName;
                private readonly string _name;

                public bool Matches(INavigatable navigatable)
                {
                    return (navigatable is Node node) ? Matches(node) : false;
                }

                private bool Matches(Node node)
                {
                    return node.Kind == _nodeKind && node.Symbol != null &&
                        node.Symbol.Name == _name &&
                        node.Symbol.ContainingType.Equals(node.Compilation.GetTypeByMetadataName(_containingTypeMetadataName));
                }

                public bool NavigationSuggested { get; }

                public  bool ShouldExpand { get; }
            }

            public override INavigatableMarker CreateMarker(bool navigationSuggested)
            {
                return Symbol == null ? null : new Marker(Kind, Symbol, Name, navigationSuggested);
            }

            public static INavigatableMarker CreateTableMarker(INamedTypeSymbol dbType, string name)
            {
                return new Marker(NodeKind.Table, dbType.GetFullyQualifiedMetadataName(), name, true, true);
            }


            public virtual bool CanAddRelationship
            {
                get { return false; }
            }

            public virtual Document AddRelationship(string name, IPropertySymbol foreignKey, IPropertySymbol refTable,
                string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
            {
                return Mapper.Document;
            }
        }

        public INavigatableMarker CreateTableMarker(string name)
        {
            return Node.CreateTableMarker(DbType, name);
        }
    }
}
