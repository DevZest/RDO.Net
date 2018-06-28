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
            context.RegisterSyntaxNodeAction(AnalyzeMounterRegistration, SyntaxKind.InvocationExpression);
            context.RegisterSymbolAction(AnalyzeModelProperty, SymbolKind.Property);
        }

        private static void AnalyzeMounterRegistration(SyntaxNodeAnalysisContext context)
        {
            var diagnostic = AnalyzeMounterRegistration(context, (InvocationExpressionSyntax)context.Node);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeMounterRegistration(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
        {
            var semanticModel = context.SemanticModel;
            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol symbol))
                return null;

            if (!symbol.IsMounterRegistration())
                return null;

            var isColumnRegistration = symbol.Name == "RegisterColumn";

            var initializerSyntax = VerifyInvocation(invocationExpression, semanticModel, out var containingType, out var fieldSymbol);
            if (initializerSyntax == null)
                return Diagnostic.Create(Rules.MounterRegistration_InvalidInvocation, invocationExpression.GetLocation());

            var firstArgument = invocationExpression.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, containingType, out var propertySymbol))
                return Diagnostic.Create(Rules.MounterRegistration_InvalidGetter, firstArgument.GetLocation());

            if (isColumnRegistration && propertySymbol.Type.MetadataName == "LocalColumn`1")
                return Diagnostic.Create(Rules.MounterRegistration_InvalidLocalColumn, firstArgument.GetLocation(), propertySymbol.Name);

            if (AnyDuplicate(invocationExpression, propertySymbol, context.Compilation))
                return Diagnostic.Create(Rules.MounterRegistration_Duplicate, invocationExpression.GetLocation(), propertySymbol.Name);

            if (fieldSymbol != null)
            {
                var expectedMounterName = "_" + propertySymbol.Name;
                if (fieldSymbol.Name != expectedMounterName)
                    return Diagnostic.Create(Rules.MounterRegistration_MounterNaming, initializerSyntax.GetLocation(), fieldSymbol.Name, propertySymbol.Name, expectedMounterName);
            }
            return null;
        }

        private static SyntaxNode VerifyInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol)
        {
            var result = VerifyStaticFieldInitializer(expression, semanticModel, out fieldSymbol);
            if (result != null)
            {
                containingType = fieldSymbol.ContainingType;
                return result;
            }
            return VerifyStaticConstructorInvocation(expression, semanticModel, out containingType, out fieldSymbol);
        }

        private static SyntaxNode VerifyStaticFieldInitializer(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol)
        {
            fieldSymbol = null;

            if (!(expression.Parent is EqualsValueClauseSyntax equalsValueClause))
                return null;
            if (!(equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator))
                return null;

            if (semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol result && result.IsStatic)
            {
                fieldSymbol = result;
                return variableDeclarator;
            }
            return null;
        }

        private static SyntaxNode VerifyStaticConstructorInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol)
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
                return null;

            if (!(expressionStatement.Parent is BlockSyntax blockSyntax))
                return null;

            if (!(blockSyntax.Parent is ConstructorDeclarationSyntax constructorDeclaration))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (symbol == null || !symbol.IsStatic)
                return null;
            containingType = symbol.ContainingType;

            if (assignmentExpression != null)
            {
                if (!(assignmentExpression.Left is IdentifierNameSyntax identifierName))
                    return null;

                if (semanticModel.GetSymbolInfo(identifierName).Symbol is IFieldSymbol result && result.IsStatic && result.ContainingType == containingType)
                {
                    fieldSymbol = result;
                    return assignmentExpression;
                }

                return null;
            }

            return expression;
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
            var result = GetMounterRegistration(propertySymbol, classDeclaration, semanticModel);
            return result == null ? false : CompareLocation(invocationExpression, result) > 0;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            var result = GetMounterRegistration(propertySymbol, constructorDeclaration, semanticModel);
            return result == null ? false : CompareLocation(invocationExpression, result) > 0;
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

        private static void AnalyzeModelProperty(SymbolAnalysisContext context)
        {
            var diagnostic = AnalyzeModelProperty((IPropertySymbol)context.Symbol, context.Compilation);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeModelProperty(IPropertySymbol propertySymbol, Compilation compilation)
        {
            var containingType = propertySymbol.ContainingType;
            if (!containingType.IsMountable())
                return null;
            if (!propertySymbol.IsMountable())
                return null;

            var syntaxReferences = containingType.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var classDeclaration = (ClassDeclarationSyntax)syntaxReferences[i].GetSyntax();
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (GetMounterRegistration(propertySymbol, classDeclaration, semanticModel) != null)
                    return null;
            }

            return Diagnostic.Create(Rules.MounterRegistration_Missing, propertySymbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(), propertySymbol.Name);
        }

        private static InvocationExpressionSyntax GetMounterRegistration(IPropertySymbol propertySymbol, ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var members = classDeclaration.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member is ConstructorDeclarationSyntax constructorDeclaration)
                {
                    var result = GetMounterRegistration(propertySymbol, constructorDeclaration, semanticModel);
                    if (result != null)
                        return result;
                }
                if (member is FieldDeclarationSyntax fieldDeclaration)
                {
                    var result = GetMounterRegistration(propertySymbol, fieldDeclaration, semanticModel);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private static InvocationExpressionSyntax GetMounterRegistration(IPropertySymbol propertySymbol, ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (symbol == null || !symbol.IsStatic)
                return null;

            var statements = constructorDeclaration.Body.Statements;
            for (int i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];
                if (!(statement is ExpressionStatementSyntax expressionStatement))
                    continue;

                var expression = expressionStatement.Expression;
                if (expression is AssignmentExpressionSyntax assignmentExpression)
                {
                    if (assignmentExpression.Right is InvocationExpressionSyntax invocationExpression && IsMounterRegistration(invocationExpression, propertySymbol, semanticModel))
                        return invocationExpression;
                }
                else if (expression is InvocationExpressionSyntax invocationExpression && IsMounterRegistration(invocationExpression, propertySymbol, semanticModel))
                    return invocationExpression;
            }

            return null;
        }

        private static InvocationExpressionSyntax GetMounterRegistration(IPropertySymbol propertySymbol, FieldDeclarationSyntax fieldDeclaration, SemanticModel semanticModel)
        {
            var variables = fieldDeclaration.Declaration.Variables;
            if (variables.Count != 1)
                return null;
            var variableDeclarator = variables[0];
            var initializer = variableDeclarator.Initializer;
            if (initializer == null)
                return null;
            var fieldSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator) as IFieldSymbol;
            if (fieldSymbol == null || !fieldSymbol.IsStatic)
                return null;

            var result = initializer.Value as InvocationExpressionSyntax;
            return IsMounterRegistration(result, propertySymbol, semanticModel) ? result : null;
        }

        private static bool IsMounterRegistration(InvocationExpressionSyntax expression, IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol symbol))
                return false;

            if (!symbol.IsMounterRegistration())
                return false;

            var firstArgument = expression.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, propertySymbol.ContainingType, out var otherPropertySymbol))
                return false;

            return propertySymbol.Name == otherPropertySymbol.Name;
        }
    }
}
