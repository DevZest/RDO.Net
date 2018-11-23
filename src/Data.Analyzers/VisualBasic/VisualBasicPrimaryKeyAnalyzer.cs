using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class VisualBasicPrimaryKeyAnalyzer : PrimaryKeyAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeCandidateKey, SyntaxKind.ClassBlock);
            context.RegisterSyntaxNodeAction(AnalyzePrimaryKeyCreation, SyntaxKind.FunctionBlock);
        }

        private static void AnalyzeCandidateKey(SyntaxNodeAnalysisContext context)
        {
            AnalyzeCandidateKey(context, (ClassBlockSyntax)context.Node);
        }

        private static void AnalyzeCandidateKey(SyntaxNodeAnalysisContext context, ClassBlockSyntax classBlock)
        {
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classBlock);
            if (!IsCandidateKey(context, classSymbol))
                return;

            var constructorParams = VerifyConstructor(context, classSymbol, out var constructorSymbol);
            if (constructorParams.IsEmpty)
                return;

            var subNewStatement = (SubNewStatementSyntax)constructorSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            var constructorBlock = (ConstructorBlockSyntax)subNewStatement.Parent;
            VerifyBaseConstructorInitializer(context, constructorBlock, constructorParams);
        }

        private static void VerifyBaseConstructorInitializer(SyntaxNodeAnalysisContext context, ConstructorBlockSyntax constructorBlock,
            ImmutableArray<IParameterSymbol> constructorParams)
        {
            var initializer = GetInitializer(constructorBlock);
            if (initializer == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyMissingBaseConstructor, constructorBlock.SubNewStatement.NewKeyword.GetLocation()));
                return;
            }

            var arguments = initializer.ArgumentList.Arguments;
            if (arguments.Count != constructorParams.Length)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyMismatchBaseConstructor, initializer.GetLocation(), constructorParams.Length));
                return;
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var argumentExpression = arguments[i].GetExpression();
                var constructorParam = constructorParams[i];
                var sortDirection = GetSortDirection(argumentExpression, constructorParam, context.SemanticModel);
                if (!sortDirection.HasValue)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyMismatchBaseConstructorArgument, argumentExpression.GetLocation(), constructorParam.Name));
                else
                    VerifyMismatchSortAttribute(context, constructorParam, sortDirection.Value);
            }
        }

        private static InvocationExpressionSyntax GetInitializer(ConstructorBlockSyntax constructorBlock)
        {
            var statements = constructorBlock.Statements;
            if (statements.Count == 0 || !(statements[0] is ExpressionStatementSyntax expressionStatement))
                return null;

            if (!(expressionStatement.Expression is InvocationExpressionSyntax invocationExpression))
                return null;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess))
                return null;

            if (!(memberAccess.Expression is MyBaseExpressionSyntax))
                return null;

            return invocationExpression;
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
            AnalyzePrimaryKeyCreation(context, (MethodBlockSyntax)context.Node);
        }

        private static void AnalyzePrimaryKeyCreation(SyntaxNodeAnalysisContext context, MethodBlockSyntax methodDeclaration)
        {
            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            var parameters = methodSymbol.GetKeyConstructorParams(context.Compilation, out _);
            if (parameters.IsDefaultOrEmpty)
                return;

            var arguments = methodDeclaration.GetKeyConstructorArguments(parameters);
            if (arguments == null)
                return;

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                if (!(semanticModel.GetSymbolInfo(argument.GetExpression()).Symbol is IPropertySymbol propertySymbol) ||
                    propertySymbol.ContainingType != methodSymbol.ContainingType)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyInvalidArgument, argument.GetLocation()));
                else if (propertySymbol.Name.ToLower() != parameters[i].Name.ToLower())
                    context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyArgumentNaming, argument.GetLocation(), propertySymbol.Name, parameters[i].Name));
            }
        }
    }
}
