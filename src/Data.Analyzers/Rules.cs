using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static class Rules
    {
        public static readonly DiagnosticDescriptor InvalidRegisterMounterInvocation = new DiagnosticDescriptor(
            DiagnosticIds.InvalidRegisterMounterInvocation,
            new LocalizableResourceString(nameof(Resources.InvalidRegisterMounterInvocation_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidRegisterMounterInvocation_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidRegisterMounterGetterParam = new DiagnosticDescriptor(
            DiagnosticIds.InvalidRegisterMounterGetterParam,
            new LocalizableResourceString(nameof(Resources.InvalidRegisterMounterGetterParam_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidRegisterMounterGetterParam_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidRegisterLocalColumn = new DiagnosticDescriptor(
            DiagnosticIds.InvalidRegisterLocalColumn,
            new LocalizableResourceString(nameof(Resources.InvalidRegisterLocalColumn_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidRegisterLocalColumn_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateMounterRegistration = new DiagnosticDescriptor(
            DiagnosticIds.DuplicateMounterRegistration,
            new LocalizableResourceString(nameof(Resources.DuplicateMounterRegistration_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.DuplicateMounterRegistration_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterNaming = new DiagnosticDescriptor(
            DiagnosticIds.MounterNaming,
            new LocalizableResourceString(nameof(Resources.MounterNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingMounterRegistration = new DiagnosticDescriptor(
            DiagnosticIds.MissingMounterRegistration,
            new LocalizableResourceString(nameof(Resources.MissingMounterRegistration_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MissingMounterRegistration_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);
    }
}
