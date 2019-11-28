using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ImportedNamespace
    {
        public static SyntaxNode AddImports(SyntaxNode node, IEnumerable<SyntaxNode> usings)
        {
            var root = (CompilationUnitSyntax)node;
            return root.AddImports(usings.Select(x => (ImportsStatementSyntax)x).ToArray());
        }

        private static IReadOnlyList<ImportedNamespace> GetImports(SyntaxNode root, SemanticModel semanticModel)
        {
            return GetImports(root.DescendantNodes().OfType<ImportsStatementSyntax>(), semanticModel).ToArray();
        }

        private static IEnumerable<VbImportedNamespace> GetImports(IEnumerable<ImportsStatementSyntax> imports, SemanticModel semanticModel)
        {
            foreach (var import in imports)
            {
                foreach (var importsClause in import.ImportsClauses)
                {
                    var symbol = importsClause.GetNamespaceSymbol(semanticModel);
                    if (symbol != null)
                        yield return new VbImportedNamespace(symbol, import);
                }
            }
        }

        private sealed class VbImportedNamespace : ImportedNamespace
        {
            public VbImportedNamespace(INamespaceSymbol namespaceSymbol, ImportsStatementSyntax syntaxNode)
                : base(namespaceSymbol)
            {
                _syntaxNode = syntaxNode;
            }

            private readonly ImportsStatementSyntax _syntaxNode;
            public override SyntaxNode SyntaxNode
            {
                get { return _syntaxNode; }
            }
        }
    }
}
