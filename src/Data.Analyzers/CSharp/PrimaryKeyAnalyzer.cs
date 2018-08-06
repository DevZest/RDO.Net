using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimaryKeyAnalyzer : PrimaryKeyAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzePrimaryKey, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePrimaryKeyCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzePrimaryKey(SyntaxNodeAnalysisContext context)
        {
            AnalyzePrimaryKey(context, (ClassDeclarationSyntax)context.Node);
        }

        private static void AnalyzePrimaryKey(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
        {
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (!IsPrimaryKey(context, classSymbol))
                return;

            var constructorParams = VerifyConstructor(context, classSymbol, out var constructorSymbol);
            if (constructorParams.IsEmpty)
                return;

            var constructorDeclaration = (ConstructorDeclarationSyntax)constructorSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            VerifyBaseConstructorInitializer(context, constructorDeclaration, constructorParams);
        }

        private static void VerifyBaseConstructorInitializer(SyntaxNodeAnalysisContext context, ConstructorDeclarationSyntax constructorDeclaration,
            ImmutableArray<IParameterSymbol> constructorParams)
        {
            var initializer = constructorDeclaration.Initializer;
            if (initializer == null || initializer.ThisOrBaseKeyword.Kind() != SyntaxKind.BaseKeyword)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyMissingBaseConstructor, constructorDeclaration.Identifier.GetLocation()));
                return;
            }

            var arguments = initializer.ArgumentList.Arguments;
            if (arguments.Count != constructorParams.Length)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyMismatchBaseConstructor, initializer.GetLocation(), constructorParams.Length));
                return;
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var argumentExpression = arguments[i].Expression;
                var constructorParam = constructorParams[i];
                var sortDirection = GetSortDirection(argumentExpression, constructorParam, context.SemanticModel);
                if (!sortDirection.HasValue)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyMismatchBaseConstructorArgument, argumentExpression.GetLocation(), constructorParam.Name));
                else
                    VerifyMismatchSortAttribute(context, constructorParam, sortDirection.Value);
            }
        }

        private static SortDirection? GetSortDirection(ExpressionSyntax argumentExpression, IParameterSymbol constructorParam, SemanticModel semanticModel)
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
            return GetSortDirection(expressionSymbol, name, constructorParam);
        }

        private static void AnalyzePrimaryKeyCreation(SyntaxNodeAnalysisContext context)
        {
            AnalyzePrimaryKeyCreation(context, (ObjectCreationExpressionSyntax)context.Node);
        }

        private static void AnalyzePrimaryKeyCreation(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax objectCreation)
        {
            var semanticModel = context.SemanticModel;
            if (!(semanticModel.GetDeclaredSymbol(objectCreation.Type) is INamedTypeSymbol typeSymbol))
                return;

            if (!IsPrimaryKey(context, typeSymbol))
                return;

            var arguments = objectCreation.ArgumentList.Arguments;
            
        }

        private static INamedTypeSymbol GetContainingType(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax objectCreation)
        {
            throw new System.NotImplementedException();
        }
    }
}
