using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class NamedModelAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeModelAttribute, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                  Rules.MissingMounterRegistration);
            }
        }

        private static void AnalyzeModelAttribute(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!type.IsDerivedFrom(KnownTypes.Model, context.Compilation))
                return;

            var attributes = type.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
                AnalyzeModelAttribute(context, type, attributes[i]);
        }

        private static void AnalyzeModelAttribute(SymbolAnalysisContext context, INamedTypeSymbol type, AttributeData attribute)
        {
            var compilation = context.Compilation;
            var attributeClass = attribute.AttributeClass;
            if (!attributeClass.IsDerivedFrom(KnownTypes.NamedModelAttribute, compilation))
                return;
        }
    }
}
