using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
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

            if (!IsValidInvocation(invocationExpression, semanticModel, out var containingType, out var fieldSymbol))
                return Diagnostic.Create(Rule_InvalidInvocation, invocationExpression.GetLocation());

            var firstArgument = invocationExpression.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, containingType, out var propertySymbol))
                return Diagnostic.Create(Rule_InvalidGetter, firstArgument.GetLocation());

            if (AnyDuplicate(invocationExpression, propertySymbol, context.Compilation))
                return Diagnostic.Create(Rule_Duplicate, invocationExpression.GetLocation(), propertySymbol.Name);

            if (fieldSymbol != null)
            {
                var expectedMounterName = "_" + propertySymbol.Name;
                if (fieldSymbol.Name != expectedMounterName)
                    return Diagnostic.Create(Rule_MounterNaming, fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(), fieldSymbol.Name, propertySymbol.Name, expectedMounterName);
            }
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

            var expression = argument.Expression;
            if (!(expression is LambdaExpressionSyntax lambdaExpression))
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

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, Compilation compilation)
        {
            var containingType = propertySymbol.ContainingType;
            var syntaxReferences = containingType.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var classDeclaration = (ClassDeclarationSyntax)syntaxReferences[i].GetSyntax();
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (AnyDuplicate(invocationExpression, propertySymbol, classDeclaration, semanticModel))
                    return true;
            }

            return false;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var members = classDeclaration.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member is ConstructorDeclarationSyntax constructorDeclaration)
                {
                    if (AnyDuplicate(invocationExpression, propertySymbol, constructorDeclaration, semanticModel))
                        return true;
                }
                if (member is FieldDeclarationSyntax fieldDeclaration)
                {
                    if (IsDuplicate(invocationExpression, propertySymbol, fieldDeclaration, semanticModel))
                        return true;
                }
            }

            return false;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (symbol == null || !symbol.IsStatic)
                return false;
            var containingType = symbol.ContainingType;

            var statements = constructorDeclaration.Body.Statements;
            for (int i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];
                if (!(statement is ExpressionStatementSyntax expressionStatement))
                    continue;

                var expression = expressionStatement.Expression;
                if (expression is AssignmentExpressionSyntax assignmentExpression)
                {
                    if (assignmentExpression.Right is InvocationExpressionSyntax other && IsDuplicate(invocationExpression, propertySymbol, other, semanticModel))
                        return true;
                }
                else if (expression is InvocationExpressionSyntax other)
                {
                    if (IsDuplicate(invocationExpression, propertySymbol, other, semanticModel))
                        return true;
                }
            }

            return false;
        }

        private static bool IsDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, InvocationExpressionSyntax other, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetSymbolInfo(other).Symbol is IMethodSymbol symbol))
                return false;

            if (!IsMounterRegistration(symbol))
                return false;

            var firstArgument = other.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, propertySymbol.ContainingType, out var otherPropertySymbol))
                return false;

            return propertySymbol.Name == otherPropertySymbol.Name && CompareLocation(invocationExpression, other) > 0;
        }

        private static int CompareLocation(InvocationExpressionSyntax x, InvocationExpressionSyntax y)
        {
            if (x == y)
                return 0;

            var result = Comparer<string>.Default.Compare(x.SyntaxTree.FilePath, y.SyntaxTree.FilePath);
            if (result != 0)
                return result;
            return Comparer<int>.Default.Compare(x.GetLocation().SourceSpan.Start, y.GetLocation().SourceSpan.Start);
        }

        private static bool IsDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, FieldDeclarationSyntax fieldDeclaration, SemanticModel semanticModel)
        {
            var variables = fieldDeclaration.Declaration.Variables;
            if (variables.Count != 1)
                return false;
            var variableDeclarator = variables[0];
            var initializer = variableDeclarator.Initializer;
            if (initializer == null)
                return false;
            var fieldSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator) as IFieldSymbol;
            if (fieldSymbol == null || !fieldSymbol.IsStatic)
                return false;

            if (initializer.Value is InvocationExpressionSyntax other)
                return IsDuplicate(invocationExpression, propertySymbol, other, semanticModel);

            return false;
        }
    }
}
