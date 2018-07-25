using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class PrimaryKeyAnalyzer : PrimaryKeyAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzePrimaryKey, SyntaxKind.ClassBlock);
        }

        private static void AnalyzePrimaryKey(SyntaxNodeAnalysisContext context)
        {
            AnalyzePrimaryKey(context, (ClassBlockSyntax)context.Node);
        }

        private static void AnalyzePrimaryKey(SyntaxNodeAnalysisContext context, ClassBlockSyntax classBlock)
        {
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classBlock);
            if (!IsPrimaryKey(context, classSymbol))
                return;

            var constructorParams = VerifyConstructor(context, classSymbol, out var constructorSymbol);
            if (constructorParams.IsEmpty)
                return;
        }

    }
}
