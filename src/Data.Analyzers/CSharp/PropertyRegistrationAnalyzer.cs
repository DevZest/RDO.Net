using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyRegistrationAnalyzer : PropertyRegistrationAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeRegisterInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeModelProperty, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeRegisterInvocation(SyntaxNodeAnalysisContext context)
        {
            var diagnostic = AnalyzeRegisterInvocation(context, (InvocationExpressionSyntax)context.Node);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeRegisterInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
        {
            var semanticModel = context.SemanticModel;
            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol symbol))
                return null;

            if (!symbol.IsPropertyRegistration(context.Compilation))
                return null;

            var isColumnRegistration = symbol.Name == "RegisterColumn";

            if (!VerifyInvocation(invocationExpression, semanticModel, out var containingType, out var fieldSymbol, out var mounterIdentifier))
                return Diagnostic.Create(Rules.InvalidRegistrationInvocation, invocationExpression.GetLocation());

            var arguments = invocationExpression.ArgumentList.Arguments;
            var firstArgument = arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, containingType, out var propertySymbol, out var nameSyntax))
                return Diagnostic.Create(Rules.InvalidRegistrationGetterParam, firstArgument.GetLocation());

            if (isColumnRegistration && propertySymbol.Type.EqualsTo(KnownTypes.LocalColumn, context.Compilation))
                return Diagnostic.Create(Rules.InvalidLocalColumnRegistration, invocationExpression.GetLocation(), propertySymbol.Name);

            if (AnyDuplicate(invocationExpression, propertySymbol, context.Compilation))
                return Diagnostic.Create(Rules.DuplicateRegistration, invocationExpression.GetLocation(), propertySymbol.Name);

            if (fieldSymbol != null)
            {
                var expectedMounterName = "_" + propertySymbol.Name;
                if (fieldSymbol.Name != expectedMounterName)
                    return Diagnostic.Create(Rules.MounterNaming, mounterIdentifier.GetLocation(), fieldSymbol.Name, propertySymbol.Name, expectedMounterName);
            }
            else if (symbol.Name == "Register" && arguments.Count == 2) // Projection column registration
            {
                var fromMounter = semanticModel.GetSymbolInfo(arguments[1].Expression).Symbol as IFieldSymbol;
                if (fromMounter != null && "_" + propertySymbol.Name != fromMounter.Name)
                    return Diagnostic.Create(Rules.ProjectionColumnNaming, nameSyntax.GetLocation(), propertySymbol.Name, fromMounter.Name);
            }
            return null;
        }

        private static bool VerifyInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            if (VerifyStaticFieldInitializer(expression, semanticModel, out fieldSymbol, out mounterIdentifier))
            {
                containingType = fieldSymbol.ContainingType;
                return true;
            }
            return VerifyStaticConstructorInvocation(expression, semanticModel, out containingType, out fieldSymbol, out mounterIdentifier);
        }

        private static bool VerifyStaticFieldInitializer(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            fieldSymbol = null;
            mounterIdentifier = default(SyntaxToken);

            if (!(expression.Parent is EqualsValueClauseSyntax equalsValueClause))
                return false;
            if (!(equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator))
                return false;

            if (semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol result && result.IsStatic)
            {
                fieldSymbol = result;
                mounterIdentifier = variableDeclarator.Identifier;
                return true;
            }
            return false;
        }

        private static bool VerifyStaticConstructorInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            containingType = null;
            fieldSymbol = null;
            mounterIdentifier = default(SyntaxToken);

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
                    mounterIdentifier = identifierName.Identifier;
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool IsValidGetter(ArgumentSyntax argument, SemanticModel semanticModel, INamedTypeSymbol containingType, out IPropertySymbol propertySymbol, out SimpleNameSyntax nameSyntax)
        {
            propertySymbol = null;
            nameSyntax = null;

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
            nameSyntax = memberAccessExpression.Name;
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

        private static void AnalyzeModelProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

            var diagnostic = AnalyzeModelProperty(propertyDeclaration.Identifier, propertySymbol, context.Compilation);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeModelProperty(SyntaxToken identifier, IPropertySymbol propertySymbol, Compilation compilation)
        {
            if (propertySymbol.SetMethod == null)
                return null;

            if (!propertySymbol.GetPropertyRegistrationType(compilation).HasValue)
                return null;

            var syntaxReferences = propertySymbol.ContainingType.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var classDeclaration = (ClassDeclarationSyntax)syntaxReferences[i].GetSyntax();
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (GetMounterRegistration(propertySymbol, classDeclaration, semanticModel) != null)
                    return null;
            }

            return Diagnostic.Create(Rules.MissingRegistration, identifier.GetLocation(), propertySymbol.Name);
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
            if (!(semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol fieldSymbol) || !fieldSymbol.IsStatic)
                return null;

            var result = initializer.Value as InvocationExpressionSyntax;
            return IsMounterRegistration(result, propertySymbol, semanticModel) ? result : null;
        }

        private static bool IsMounterRegistration(InvocationExpressionSyntax expression, IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            if (expression == null)
                return false;

            if (!(semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol symbol))
                return false;

            if (!symbol.IsPropertyRegistration(semanticModel.Compilation))
                return false;

            var firstArgument = expression.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, propertySymbol.ContainingType, out var otherPropertySymbol, out _))
                return false;

            return propertySymbol.Name == otherPropertySymbol.Name;
        }
    }
}
