using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    public abstract class MounterRegistrationAnalyzerBase : DiagnosticAnalyzer
    {
        private static readonly LocalizableString String_InvalidInvocation = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidInvocation), Resources.ResourceManager, typeof(Resources));

        protected static readonly DiagnosticDescriptor Rule_InvalidInvocation = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidInvocation, String_InvalidInvocation, String_InvalidInvocation, DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule_InvalidInvocation); }
        }

        protected static bool IsMounterRegistration(IMethodSymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => TypeIdentifier.MounterRegistrationAttribute.IsSameType(x.AttributeClass));
        }
    }
}
