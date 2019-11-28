using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Composition;
using System.Threading;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic, Name = nameof(VisualBasicDbMockCodeFixProvider)), Shared]
    public class VisualBasicDbMockCodeFixProvider : DbMockCodeFixProvider
    {
        protected override INamedTypeSymbol GetClassSymbol(SyntaxNode root, SemanticModel semanticModel, TextSpan diagnosticSpan, out SyntaxNode classNode, CancellationToken ct)
        {
            var classBlock = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf<ClassBlockSyntax>();
            classNode = classBlock;
            return semanticModel.GetDeclaredSymbol(classBlock, ct);
        }
    }
}
