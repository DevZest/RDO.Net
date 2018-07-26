using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static SortDirection? GetSortDirection(this ExpressionSyntax argumentExpression, IParameterSymbol constructorParam, SemanticModel semanticModel)
        {
            var expressionSymbol = semanticModel.GetSymbolInfo(argumentExpression).Symbol;
            if (expressionSymbol == null)
                return null;

            if (expressionSymbol == constructorParam)
                return SortDirection.Unspecified;

            if (!(argumentExpression is InvocationExpressionSyntax invocationSyntax))
                return null;

            if (!(invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax))
                return null;

            if (memberAccessSyntax.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return null;

            var name = expressionSymbol.Name;
            expressionSymbol = semanticModel.GetSymbolInfo(memberAccessSyntax.Expression).Symbol;
            if (expressionSymbol == null || expressionSymbol != constructorParam)
                return null;

            if (name == "Asc")
                return SortDirection.Ascending;
            else if (name == "Desc")
                return SortDirection.Descending;
            else
                return null;
        }
    }
}
