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
            if (initializer == null)
                return;
            if (initializer.ThisOrBaseKeyword.Kind() != SyntaxKind.BaseKeyword)
                return;
            var arguments = initializer.ArgumentList.Arguments;
            if (arguments.Count != constructorParams.Length)
            {
                //context.ReportDiagnostic();
                return;
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var sortOrder = arguments[i].Expression.GetSortDirection(constructorParams[i], context.SemanticModel);
                if (!sortOrder.HasValue)
                {
                    //context.ReportDiagnostic();
                }
            }

        }
    }
}
