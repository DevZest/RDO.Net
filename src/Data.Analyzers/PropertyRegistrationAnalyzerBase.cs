using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PropertyRegistrationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                Rules.InvalidRegisterMounterInvocation, 
                Rules.InvalidRegistrationGetterParam,
                Rules.InvalidRegisterLocalColumn,
                Rules.DuplicateMounterRegistration,
                Rules.MounterNaming,
                Rules.ProjectionColumnNaming,
                Rules.MissingMounterRegistration); }
        }
    }
}
