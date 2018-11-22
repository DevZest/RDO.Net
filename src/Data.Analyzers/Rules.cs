using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static class Rules
    {
        public static readonly DiagnosticDescriptor InvalidRegistrationInvocation = new DiagnosticDescriptor(
            DiagnosticIds.InvalidRegistrationInvocation,
            new LocalizableResourceString(nameof(Resources.InvalidRegistrationInvocation_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidRegistrationInvocation_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidRegistrationGetterParam = new DiagnosticDescriptor(
            DiagnosticIds.InvalidRegistrationGetterParam,
            new LocalizableResourceString(nameof(Resources.InvalidRegistrationGetterParam_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidRegistrationGetterParam_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidLocalColumnRegistration = new DiagnosticDescriptor(
            DiagnosticIds.InvalidLocalColumnRegistration,
            new LocalizableResourceString(nameof(Resources.InvalidLocalColumnRegistration_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidLocalColumnRegistration_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateRegistration = new DiagnosticDescriptor(
            DiagnosticIds.DuplicateRegistration,
            new LocalizableResourceString(nameof(Resources.DuplicateRegistration_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.DuplicateRegistration_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MounterNaming = new DiagnosticDescriptor(
            DiagnosticIds.MounterNaming,
            new LocalizableResourceString(nameof(Resources.MounterNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MounterNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ProjectionColumnNaming = new DiagnosticDescriptor(
            DiagnosticIds.ProjectionColumnNaming,
            new LocalizableResourceString(nameof(Resources.ProjectionColumnNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ProjectionColumnNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingRegistration = new DiagnosticDescriptor(
            DiagnosticIds.MissingRegistration,
            new LocalizableResourceString(nameof(Resources.MissingRegistration_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MissingRegistration_Message), Resources.ResourceManager, typeof(Resources)),
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

        public static readonly DiagnosticDescriptor PrimaryKeyInvalidArgument = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyInvalidArgument,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidArgument_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyInvalidArgument_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrimaryKeyArgumentNaming = new DiagnosticDescriptor(
            DiagnosticIds.PrimaryKeyArgumentNaming,
            new LocalizableResourceString(nameof(Resources.PrimaryKeyArgumentNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PrimaryKeyArgumentNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingDeclarationAttribute = new DiagnosticDescriptor(
            DiagnosticIds.MissingDeclarationAttribute,
            new LocalizableResourceString(nameof(Resources.MissingDeclarationAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MissingDeclarationAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateDeclarationAttribute = new DiagnosticDescriptor(
            DiagnosticIds.DuplicateDeclarationAttribute,
            new LocalizableResourceString(nameof(Resources.DuplicateDeclarationAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.DuplicateDeclarationAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingImplementation = new DiagnosticDescriptor(
            DiagnosticIds.MissingImplementation,
            new LocalizableResourceString(nameof(Resources.MissingImplementation_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MissingImplementation_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingImplementationAttribute = new DiagnosticDescriptor(
            DiagnosticIds.MissingImplementationAttribute,
            new LocalizableResourceString(nameof(Resources.MissingImplementationAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MissingImplementationAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ModelDesignerSpecInvalidType = new DiagnosticDescriptor(
            DiagnosticIds.ModelDesignerSpecInvalidType,
            new LocalizableResourceString(nameof(Resources.ModelDesignerSpecInvalidType_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ModelDesignerSpecInvalidType_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ModelDesignerSpecRequiresArgument = new DiagnosticDescriptor(
            DiagnosticIds.ModelDesignerSpecRequiresArgument,
            new LocalizableResourceString(nameof(Resources.ModelDesignerSpecRequiresArgument_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ModelDesignerSpecRequiresArgument_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);
    }
}
