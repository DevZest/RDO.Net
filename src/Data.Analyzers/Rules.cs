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

        public static readonly DiagnosticDescriptor CandidateKeyNotSealed = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyNotSealed,
            new LocalizableResourceString(nameof(Resources.CandidateKeyNotSealed_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyNotSealed_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyInvalidConstructors = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyInvalidConstructors,
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidConstructors_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidConstructors_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyParameterlessConstructor = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyParameterlessConstructor,
            new LocalizableResourceString(nameof(Resources.CandidateKeyParameterlessConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyParameterlessConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyInvalidConstructorParam = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyInvalidConstructorParam,
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidConstructorParam_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidConstructorParam_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyMissingBaseConstructor = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyMissingBaseConstructor,
            new LocalizableResourceString(nameof(Resources.CandidateKeyMissingBaseConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyMissingBaseConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeySortAttributeConflict = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeySortAttributeConflict,
            new LocalizableResourceString(nameof(Resources.CandidateKeySortAttributeConflict_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeySortAttributeConflict_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyMismatchBaseConstructor = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyMismatchBaseConstructor,
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchBaseConstructor_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchBaseConstructor_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyMismatchBaseConstructorArgument = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyMismatchBaseConstructorArgument,
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchBaseConstructorArgument_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchBaseConstructorArgument_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyMismatchSortAttribute = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyMismatchSortAttribute,
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchSortAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyMismatchSortAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyInvalidArgument = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyInvalidArgument,
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidArgument_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyInvalidArgument_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CandidateKeyArgumentNaming = new DiagnosticDescriptor(
            DiagnosticIds.CandidateKeyArgumentNaming,
            new LocalizableResourceString(nameof(Resources.CandidateKeyArgumentNaming_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.CandidateKeyArgumentNaming_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Naming, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidImplementationAttribute = new DiagnosticDescriptor(
            DiagnosticIds.InvalidImplementationAttribute,
            new LocalizableResourceString(nameof(Resources.InvalidImplementationAttribute_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidImplementationAttribute_Message), Resources.ResourceManager, typeof(Resources)),
            DiagnosticCategories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

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
