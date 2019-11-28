using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DevZest.Data.DbInit
{
    static partial class Extensions
    {
        public static SyntaxNode VbSimpleAssignment(this SyntaxNode left, SyntaxNode right)
        {
            var equalsToken = SyntaxFactory.Token(SyntaxKind.EqualsToken);
            return SyntaxFactory.AssignmentStatement(SyntaxKind.SimpleAssignmentStatement, (ExpressionSyntax)left, equalsToken, (ExpressionSyntax)right);
        }
    }
}
