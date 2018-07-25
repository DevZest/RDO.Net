using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PrimaryKeyAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                Rules.PrimaryKeyNotSealed,
                Rules.PrimaryKeyInvalidConstructors); }
        }

        protected static void VerifyConstructors(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol, SyntaxToken identifier)
        {
            if (classSymbol.Constructors.Length != 1 || classSymbol.Constructors[0].IsStatic || classSymbol.Constructors[0].IsImplicitlyDeclared)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructors, identifier.GetLocation()));
        }
    }
}
