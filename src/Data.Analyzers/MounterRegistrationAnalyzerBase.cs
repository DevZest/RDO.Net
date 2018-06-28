using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    public abstract class MounterRegistrationAnalyzerBase : DiagnosticAnalyzer
    {
        private static readonly LocalizableString InvalidInvocation_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidInvocation_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidInvocation_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidInvocation_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidGetter_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidGetter_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidGetter_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidGetter_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Duplicate_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_Duplicate_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Duplicate_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_Duplicate_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MounterNaming_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_MounterNaming_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MounterNaming_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_MounterNaming_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidLocalColumn_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidLocalColumn_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidLocalColumn_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidLocalColumn_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingRegistration_Title = new LocalizableResourceString(nameof(Resources.MounterRegistration_Missing_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingRegistration_Message = new LocalizableResourceString(nameof(Resources.MounterRegistration_Missing_Message), Resources.ResourceManager, typeof(Resources));

        protected static readonly DiagnosticDescriptor Rule_InvalidInvocation = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidInvocation, InvalidInvocation_Title, InvalidInvocation_Message, DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        protected static readonly DiagnosticDescriptor Rule_InvalidGetter = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidGetter, InvalidGetter_Title, InvalidGetter_Message, DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        protected static readonly DiagnosticDescriptor Rule_InvalidLocalColumn = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidLocalColumn, InvalidLocalColumn_Title, InvalidLocalColumn_Message, DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        protected static readonly DiagnosticDescriptor Rule_Duplicate = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_Duplicate, Duplicate_Title, Duplicate_Message, DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        protected static readonly DiagnosticDescriptor Rule_MounterNaming = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_MounterNaming, MounterNaming_Title, MounterNaming_Message, DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        protected static readonly DiagnosticDescriptor Rule_MissingRegistration = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_Missing, MissingRegistration_Title, MissingRegistration_Message, DiagnosticCategories.Compile, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule_InvalidInvocation, Rule_InvalidGetter, Rule_InvalidLocalColumn, Rule_Duplicate, Rule_MounterNaming, Rule_MissingRegistration); }
        }
    }
}
