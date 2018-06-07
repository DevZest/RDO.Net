using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;
using System.Linq;

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

            if (!IsValidInvocation(invocationExpression, semanticModel, out var containingType, out var fieldSymbol))
                return Diagnostic.Create(Rule_InvalidInvocation, invocationExpression.GetLocation());

            var firstArgument = invocationExpression.ArgumentList.ChildNodes().Where(x => x.Kind() == SyntaxKind.Argument).First() as ArgumentSyntax;
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, containingType, out var propertySymbol))
                return Diagnostic.Create(Rule_InvalidGetter, firstArgument.GetLocation());

            return null;
        }

        private static bool IsValidInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol)
        {
            if (IsStaticFieldInitializer(expression, semanticModel, out fieldSymbol))
            {
                containingType = fieldSymbol.ContainingType;
                return true;
            }
            return IsStaticConstructorInvocation(expression, semanticModel, out containingType, out fieldSymbol);
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

        private static bool IsStaticConstructorInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol)
        {
            containingType = null;
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
            containingType = symbol.ContainingType;

            if (assignmentExpression != null)
            {
                if (!(assignmentExpression.Left is IdentifierNameSyntax identifierName))
                    return false;

                if (semanticModel.GetSymbolInfo(identifierName).Symbol is IFieldSymbol result && result.IsStatic && result.ContainingType == containingType)
                {
                    fieldSymbol = result;
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool IsValidGetter(ArgumentSyntax argument, SemanticModel semanticModel, INamedTypeSymbol containingType, out IPropertySymbol propertySymbol)
        {
            propertySymbol = null;

            if (argument.ChildNodes().Count() != 1)
                return false;

            if (!(argument.ChildNodes().Single() is LambdaExpressionSyntax lambdaExpression))
                return false;

            if (!(lambdaExpression.Body is MemberAccessExpressionSyntax memberAccessExpression))
                return false;

            if (!memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return false;

            if (!(semanticModel.GetSymbolInfo(memberAccessExpression).Symbol is IPropertySymbol result))
                return false;

            if (result.ContainingType != containingType)
                return false;

            if (result.SetMethod == null)
                return false;

            propertySymbol = result;
            return true;
        }
    }
}
