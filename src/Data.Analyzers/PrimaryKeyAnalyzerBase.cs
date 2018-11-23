using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PrimaryKeyAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    Rules.CandidateKeyNotSealed,
                    Rules.CandidateKeyInvalidConstructors,
                    Rules.CandidateKeyParameterlessConstructor,
                    Rules.CandidateKeyInvalidConstructorParam,
                    Rules.CandidateKeyMissingBaseConstructor,
                    Rules.CandidateKeySortAttributeConflict,
                    Rules.CandidateKeyMismatchBaseConstructor,
                    Rules.CandidateKeyMismatchBaseConstructorArgument,
                    Rules.CandidateKeyMismatchSortAttribute,
                    Rules.CandidateKeyInvalidArgument,
                    Rules.CandidateKeyArgumentNaming);
            }
        }

        protected static bool IsCandidateKey(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            return classSymbol != null && classSymbol.BaseTypeEqualsTo(KnownTypes.CandidateKey, context.Compilation);
        }

        private static void VerifySealed(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.IsSealed)
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyNotSealed, classSymbol.Locations[0]));
        }

        protected static ImmutableArray<IParameterSymbol> VerifyConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol, out IMethodSymbol constructorSymbol)
        {
            VerifySealed(context, classSymbol);

            constructorSymbol = classSymbol.GetSingleConstructor();
            if (constructorSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyInvalidConstructors, classSymbol.Locations[0]));
                return ImmutableArray<IParameterSymbol>.Empty;
            }

            var parameters = constructorSymbol.Parameters;
            if (parameters.IsEmpty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyParameterlessConstructor, constructorSymbol.Locations[0]));
                return parameters;
            }

            bool areParametersValid = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (!parameter.Type.IsDerivedFrom(KnownTypes.Column, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyInvalidConstructorParam, parameter.Locations[0], parameter.Name));
                    areParametersValid = false;
                }
                VerifyParameterAttributes(context, parameter);
            }

            return areParametersValid ? parameters : ImmutableArray<IParameterSymbol>.Empty;
        }

        private static void VerifyParameterAttributes(SyntaxNodeAnalysisContext context, IParameterSymbol parameter)
        {
            int ascAttributeIndex = -1;
            int descAttributeIndex = -1;
            var attributes = parameter.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                var attributeClass = attributes[i].AttributeClass;
                if (attributeClass.EqualsTo(KnownTypes.AscAttribute, context.Compilation))
                    ascAttributeIndex = i;
                else if (attributeClass.EqualsTo(KnownTypes.DescAttribute, context.Compilation))
                    descAttributeIndex = i;
            }

            if (ascAttributeIndex >= 0 && descAttributeIndex >= 0)
            {
                var attribute = attributes[Math.Max(ascAttributeIndex, descAttributeIndex)];
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeySortAttributeConflict, attribute.ApplicationSyntaxReference.GetSyntax().GetLocation()));
            }
        }

        protected static void VerifyMismatchSortAttribute(SyntaxNodeAnalysisContext context, IParameterSymbol parameter, SortDirection sortDirection)
        {
            var paramSortDirection = parameter.GetSortDirection(context.Compilation);
            var isMismatched = paramSortDirection.HasValue ? sortDirection != paramSortDirection.Value : false;
            if (isMismatched)
                context.ReportDiagnostic(Diagnostic.Create(Rules.CandidateKeyMismatchSortAttribute, parameter.Locations[0], paramSortDirection.Value, sortDirection));
        }

        protected static SortDirection? GetSortDirection(ISymbol expressionSymbol, string methodName, IParameterSymbol constructorParam)
        {
            if (expressionSymbol == null || expressionSymbol != constructorParam)
                return null;

            if (methodName == "Asc")
                return SortDirection.Ascending;
            else if (methodName == "Desc")
                return SortDirection.Descending;
            else
                return null;

        }
    }
}
