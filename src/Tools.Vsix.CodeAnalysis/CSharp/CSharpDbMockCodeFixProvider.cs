using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Composition;
using System.Threading;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharpDbMockCodeFixProvider)), Shared]
    public class CSharpDbMockCodeFixProvider : DbMockCodeFixProvider
    {
        protected override INamedTypeSymbol GetClassSymbol(SyntaxNode root, SemanticModel semanticModel, TextSpan diagnosticSpan, out SyntaxNode classNode, CancellationToken ct)
        {
            var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            classNode = classDeclaration;
            return semanticModel.GetDeclaredSymbol(classDeclaration, ct);
        }
    }
}
