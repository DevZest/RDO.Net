﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevZest.Data.Analyzers
{
    public abstract class MounterRegistrationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                Rules.InvalidRegisterMounterInvocation, 
                Rules.InvalidRegisterMounterGetterParam,
                Rules.InvalidRegisterLocalColumn,
                Rules.DuplicateMounterRegistration,
                Rules.MounterNaming,
                Rules.MissingMounterRegistration); }
        }
    }
}
