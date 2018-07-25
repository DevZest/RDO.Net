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
            var compilation = context.Compilation;
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol == null || !classSymbol.BaseType.Equals(compilation.TypeOfPrimaryKey()))
                return;

            if (!classSymbol.IsSealed)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyNotSealed, classDeclaration.Identifier.GetLocation()));

            VerifyConstructors(context, classSymbol, classDeclaration.Identifier);
        }

    }
}
