using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PrimaryKeyAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                Rules.PrimaryKeyNotSealed,
                Rules.PrimaryKeyInvalidConstructors,
                Rules.PrimaryKeyParameterlessConstructor); }
        }

        protected static bool IsPrimaryKey(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            return classSymbol != null && classSymbol.BaseType.Equals(context.Compilation.TypeOfPrimaryKey());
        }

        protected static void VerifySealed(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.IsSealed)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyNotSealed, classSymbol.Locations[0]));
        }

        protected static ImmutableArray<IParameterSymbol> VerifyConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            var constructor = GetPrimaryKeyConstructor(classSymbol);
            if (constructor == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructors, classSymbol.Locations[0]));
                return ImmutableArray<IParameterSymbol>.Empty;
            }

            var parameters = constructor.Parameters;
            if (parameters.Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyParameterlessConstructor, constructor.Locations[0]));
                return parameters;
            }

            return parameters;
        }

        private static IMethodSymbol GetPrimaryKeyConstructor(INamedTypeSymbol classSymbol)
        {
            if (classSymbol.Constructors.Length != 1)
                return null;

            var result = classSymbol.Constructors[0];
            return result.IsStatic || result.IsImplicitlyDeclared ? null : result;
        }
    }
}
