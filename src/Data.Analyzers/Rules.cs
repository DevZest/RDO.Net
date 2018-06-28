using Microsoft.CodeAnalysis;

namespace DevZest.Data.Analyzers
{
    internal static class Rules
    {
        public static readonly DiagnosticDescriptor MounterRegistration_InvalidInvocation = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidInvocation,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidInvocation_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidInvocation_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterRegistration_InvalidGetter = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidGetter,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidGetter_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidGetter_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterRegistration_InvalidLocalColumn = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_InvalidLocalColumn,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidLocalColumn_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_InvalidLocalColumn_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterRegistration_Duplicate = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_Duplicate,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_Duplicate_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_Duplicate_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Compile, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterRegistration_MounterNaming = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_MounterNaming,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_MounterNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_MounterNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterRegistration_Missing = new DiagnosticDescriptor(
            DiagnosticIds.MounterRegistration_Missing,
            new LocalizableResourceString(nameof(Resources.MounterRegistration_Missing_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterRegistration_Missing_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Compile, DiagnosticSeverity.Warning, isEnabledByDefault: true);


    }
}
