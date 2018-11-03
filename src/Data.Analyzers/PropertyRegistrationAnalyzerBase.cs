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
                Rules.InvalidRegistrationInvocation, 
                Rules.InvalidRegistrationGetterParam,
                Rules.InvalidLocalColumnRegistration,
                Rules.DuplicateRegistration,
                Rules.MounterNaming,
                Rules.ProjectionColumnNaming,
                Rules.MissingRegistration); }
        }
    }
}
