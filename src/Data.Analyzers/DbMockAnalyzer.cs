using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DbMockAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeMissingFactoryMethod, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rules.MissingDbMockFactoryMethod); }
        }

        private static void AnalyzeMissingFactoryMethod(SymbolAnalysisContext context)
        {
            var compilation = context.Compilation;

            var typeSymbol = (INamedTypeSymbol)context.Symbol;
            var dbType = typeSymbol.GetArgumentType(compilation.GetKnownType(KnownTypes.DbMockOf), compilation);
            if (dbType == null)
                return;

            var members = typeSymbol.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();
            for (int i = 0; i < members.Length; i++)
            {
                if (IsFactoryMethod(compilation, dbType, members[i]))
                    return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rules.MissingDbMockFactoryMethod, typeSymbol.Locations[0]));
        }

        private static bool IsFactoryMethod(Compilation compilation, ITypeSymbol dbType, IMethodSymbol method)
        {
            if (!method.IsStatic ||
                !compilation.GetTaskOf(dbType).Equals(method.ReturnType) ||
                method.IsGenericMethod)
                return false;

            var parameters = method.Parameters;
            if (parameters.Length != 3)
                return false;

            return Any(parameters, dbType) &&
                Any(parameters, compilation.GetIProgressOf(compilation.GetKnownType(KnownTypes.DbInitProgress))) &&
                Any(parameters, compilation.GetKnownType(KnownTypes.CancellationToken));
        }

        private static bool Any(ImmutableArray<IParameterSymbol> parameters, ITypeSymbol type)
        {
            return type == null ? false : parameters.Any(x => (type.Equals(x.Type)));
        }
    }
}
