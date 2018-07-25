using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

            VerifySealed(context, classSymbol);

            var constructorSymbol = VerifyConstructor(context, classSymbol);
            if (constructorSymbol == null)
                return;
        }

    }
}
