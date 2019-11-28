using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace DevZest.Data.CodeAnalysis
{
    static partial class DbInitMapper
    {
        private static class CSharp
        {
            public static void UpdateMethodBody(DocumentEditor editor, SyntaxNode syntaxNode, string src)
            {
                var methodDeclaration = syntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var body = (BlockSyntax)SyntaxFactory.ParseStatement(src).WithAdditionalAnnotations(Formatter.Annotation);
                editor.ReplaceNode(methodDeclaration.Body, body);
            }
        }
    }
}
