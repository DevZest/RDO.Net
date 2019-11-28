using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevZest.Data.DbInit
{
    static partial class Extensions
    {
        public static SyntaxNode CsSimpleAssignment(this SyntaxNode left, SyntaxNode right)
        {
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);
        }
    }
}
