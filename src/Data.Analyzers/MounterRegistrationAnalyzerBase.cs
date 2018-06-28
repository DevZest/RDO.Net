using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    public abstract class MounterRegistrationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                Rules.MounterRegistration_InvalidInvocation, 
                Rules.MounterRegistration_InvalidGetter,
                Rules.MounterRegistration_InvalidLocalColumn,
                Rules.MounterRegistration_Duplicate,
                Rules.MounterRegistration_MounterNaming,
                Rules.MounterRegistration_Missing); }
        }
    }
}
