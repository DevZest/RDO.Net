using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private abstract class SymbolNode : SymbolNodeBase
        {
            protected SymbolNode(ModelMapper mapper, ISymbol symbol)
                : base(mapper)
            {
                _symbol = symbol;
            }

            private readonly ISymbol _symbol;
            public sealed override ISymbol Symbol
            {
                get { return _symbol; }
            }
        }

        private abstract class SymbolNodeBase : Node
        {
            public static INavigatableMarker CreateMarker(NodeKind nodeKind, ISymbol containingType, string symbolName, bool navigationSuggested)
            {
                return new Marker(nodeKind, containingType.GetFullyQualifiedMetadataName(), symbolName, navigationSuggested);
            }

            private sealed class Marker : INavigatableMarker
            {
                public Marker(NodeKind nodeKind, ISymbol symbol, bool navigationSuggested)
                    : this(nodeKind, symbol.ContainingType.GetFullyQualifiedMetadataName(), symbol.Name, navigationSuggested)
                {
                }

                public Marker(NodeKind nodeKind, string containingTypeMetadataName, string symbolName, bool navigationSuggested)
                {
                    _nodeKind = nodeKind;
                    _containingTypeMetadataName = containingTypeMetadataName;
                    _symbolName = symbolName;
                    NavigationSuggested = navigationSuggested;
                }

                private readonly NodeKind _nodeKind;
                private readonly string _containingTypeMetadataName;
                private readonly string _symbolName;

                public bool Matches(INavigatable navigatable)
                {
                    return (navigatable is SymbolNodeBase symbolNode) ? Matches(symbolNode) : false;
                }

                private bool Matches(SymbolNodeBase symbolNode)
                {
                    return symbolNode.Kind == _nodeKind && symbolNode.Symbol != null &&
                        symbolNode.Symbol.Name == _symbolName &&
                        symbolNode.Symbol.ContainingType.Equals(symbolNode.Compilation.GetTypeByMetadataName(_containingTypeMetadataName));
                }

                public bool NavigationSuggested { get; private set; }

                public bool ShouldExpand => false;
            }

            protected SymbolNodeBase(ModelMapper mapper)
                : base(mapper)
            {
            }

            public override string Name
            {
                get { return Symbol.Name; }
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

            public override INavigatableMarker CreateMarker(bool navigationSuggested)
            {
                return new Marker(Kind, Symbol, navigationSuggested);
            }
        }
    }
}
