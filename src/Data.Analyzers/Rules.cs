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

        public static readonly DiagnosticDescriptor PrimaryKeyNotSealed = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyNotSealed,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyNotSealed_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyNotSealed_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyInvalidConstructors = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyInvalidConstructors,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidConstructors_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidConstructors_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyParameterlessConstructor = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyParameterlessConstructor,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyParameterlessConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyParameterlessConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyInvalidConstructorParam = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyInvalidConstructorParam,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidConstructorParam_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidConstructorParam_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyMissingBaseConstructor = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyMissingBaseConstructor,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMissingBaseConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMissingBaseConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeySortAttributeConflict = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeySortAttributeConflict,
            new LocalizableResourceString(nameof(Resources.PrimaryKeySortAttributeConflict_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeySortAttributeConflict_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyMismatchBaseConstructor = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyMismatchBaseConstructor,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchBaseConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchBaseConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyMismatchBaseConstructorArgument = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyMismatchBaseConstructorArgument,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchBaseConstructorArgument_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchBaseConstructorArgument_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyMismatchSortAttribute = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyMismatchSortAttribute,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchSortAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyMismatchSortAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);
    }
}
