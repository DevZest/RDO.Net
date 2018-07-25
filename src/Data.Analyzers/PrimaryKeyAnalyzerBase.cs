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
                Rules.PrimaryKeyParameterlessConstructor,
                Rules.PrimaryKeyInvalidConstructorParam); }
        }

        protected static bool IsPrimaryKey(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            return classSymbol != null && classSymbol.BaseType.Equals(context.Compilation.TypeOfPrimaryKey());
        }

        private static void VerifySealed(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.IsSealed)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyNotSealed, classSymbol.Locations[0]));
        }

        protected static ImmutableArray<IParameterSymbol> VerifyConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol, out IMethodSymbol constructorSymbol)
        {
            VerifySealed(context, classSymbol);

            constructorSymbol = GetPrimaryKeyConstructor(classSymbol);
            if (constructorSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructors, classSymbol.Locations[0]));
                return ImmutableArray<IParameterSymbol>.Empty;
            }

            var parameters = constructorSymbol.Parameters;
            if (parameters.Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyParameterlessConstructor, constructorSymbol.Locations[0]));
                return parameters;
            }

            bool areParametersValid = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (!parameter.Type.IsTypeOfColumn(context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructorParam, parameter.Locations[0], parameter.Name));
                    areParametersValid = false;
                }
            }

            return areParametersValid ? parameters : ImmutableArray<IParameterSymbol>.Empty;
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
