using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ImportedNamespace
    {
        public static SyntaxNode AddUsings(SyntaxNode node, IEnumerable<SyntaxNode> usings)
        {
            var root = (CompilationUnitSyntax)node;
            return root.AddUsings(usings.Select(x => (UsingDirectiveSyntax)x).ToArray());
        }

        private static IReadOnlyList<ImportedNamespace> GetUsings(SyntaxNode root, SemanticModel semanticModel)
        {
            return GetUsings(root.DescendantNodes().OfType<UsingDirectiveSyntax>(), semanticModel).ToArray();
        }

        private static IEnumerable<CsImportedNamespace> GetUsings(IEnumerable<UsingDirectiveSyntax> usings, SemanticModel semanticModel)
        {
            foreach (var @using in usings)
            {
                var nameSymbol = @using.Name;
                if (nameSymbol == null)
                    continue;
                var symbolInfo = semanticModel.GetSymbolInfo(nameSymbol);
                if (symbolInfo.Symbol is INamespaceSymbol symbol)
                    yield return new CsImportedNamespace(symbol, @using);
            }
        }

        private sealed class CsImportedNamespace : ImportedNamespace
        {
            public CsImportedNamespace(INamespaceSymbol namespaceSymbol, UsingDirectiveSyntax syntaxNode)
                : base(namespaceSymbol)
            {
                _syntaxNode = syntaxNode;
            }

            private readonly UsingDirectiveSyntax _syntaxNode;
            public override SyntaxNode SyntaxNode
            {
                get { return _syntaxNode; }
            }
        }
    }
}
