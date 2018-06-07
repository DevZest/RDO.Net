using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;

namespace DevZest.Data.Analyzers.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MounterRegistrationAnalyzer : MounterRegistrationAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var diagnostic = Analyze(context, (InvocationExpressionSyntax)context.Node);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
        {
            var semanticModel = context.SemanticModel;
            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol symbol))
                return null;

            if (!IsMounterRegistration(symbol))
                return null;

            if (!IsValidInvocation(invocationExpression, semanticModel, out var fieldSymbol))
                return Diagnostic.Create(Rule_InvalidInvocation, invocationExpression.GetLocation());

            return null;
        }

        private static bool IsValidInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol)
        {
            if (IsStaticFieldInitializer(expression, semanticModel, out fieldSymbol))
                return true;
            return IsStaticConstructorInvocation(expression, semanticModel, out fieldSymbol);
        }

        private static bool IsStaticFieldInitializer(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol)
        {
            fieldSymbol = null;

            if (!(expression.Parent is EqualsValueClauseSyntax equalsValueClause))
                return false;
            if (!(equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator))
                return false;

            if (semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol result && result.IsStatic)
            {
                fieldSymbol = result;
                return true;
            }
            return false;
        }

        private static bool IsStaticConstructorInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol)
        {
            fieldSymbol = null;

            SyntaxNode childOfExpressionStatement;
            var assignmentExpression = expression.Parent as AssignmentExpressionSyntax;
            if (assignmentExpression != null)
                childOfExpressionStatement = assignmentExpression;
            else
                childOfExpressionStatement = expression;

            if (!(childOfExpressionStatement.Parent is ExpressionStatementSyntax expressionStatement))
                return false;

            if (!(expressionStatement.Parent is BlockSyntax blockSyntax))
                return false;

            if (!(blockSyntax.Parent is ConstructorDeclarationSyntax constructorDeclaration))
                return false;

            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (symbol == null || !symbol.IsStatic)
                return false;

            if (assignmentExpression != null)
            {
                if (!(assignmentExpression.Left is IdentifierNameSyntax identifierName))
                    return false;

                if (semanticModel.GetSymbolInfo(identifierName).Symbol is IFieldSymbol result && result.IsStatic)
                {
                    fieldSymbol = result;
                    return true;
                }
            }
            return false;
        }
    }
}
