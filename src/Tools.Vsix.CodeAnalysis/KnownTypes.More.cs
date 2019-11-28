namespace DevZest.Data.CodeAnalysis
{
    static partial class KnownTypes
    {
        private static partial class Namespaces
        {
            // Must end with "."
            public const string Data_DbInit = Data + "DbInit.";
        }

        public const string KeyOf = Namespaces.Data + "Key`1";
        public const string RefOf = Namespaces.Data + "Ref`1";
        public const string MounterOf = Namespaces.Data + "Mounter`1";
        public const string Attribute = Namespaces.System + nameof(Attribute);
        public const string CheckConstraintAttribute = Namespaces.Data_Annotations + nameof(CheckConstraintAttribute);
        public const string UniqueConstraintAttribute = Namespaces.Data_Annotations + nameof(UniqueConstraintAttribute);
        public const string CustomValidatorAttribute = Namespaces.Data_Annotations + nameof(CustomValidatorAttribute);
        public const string CustomValidatorEntry = Namespaces.Data_Annotations + nameof(CustomValidatorEntry);
        public const string DbIndexAttribute = Namespaces.Data_Annotations + nameof(DbIndexAttribute);
        public const string _CheckConstraintAttribute = Namespaces.Data_Annotations + nameof(_CheckConstraintAttribute);
        public const string _UniqueConstraintAttribute = Namespaces.Data_Annotations + nameof(_UniqueConstraintAttribute);
        public const string _CustomValidatorAttribute = Namespaces.Data_Annotations + nameof(_CustomValidatorAttribute);
        public const string _DbIndexAttribute = Namespaces.Data_Annotations + nameof(_DbIndexAttribute);
        public const string ColumnSort = Namespaces.Data + nameof(ColumnSort);
        public const string MessageResourceAttribute = Namespaces.Data_Annotations + nameof(MessageResourceAttribute);
        public const string _Boolean = Namespaces.Data + nameof(_Boolean);
        public const string NotImplementedException = Namespaces.System + nameof(NotImplementedException);
        public const string ComputationMode = Namespaces.Data_Annotations + nameof(ComputationMode);
        public const string ComputationAttribute = Namespaces.Data_Annotations + nameof(ComputationAttribute);
        public const string _ComputationAttribute = Namespaces.Data_Annotations + nameof(_ComputationAttribute);
        public const string DataRow = Namespaces.Data + nameof(DataRow);
        public const string DataValidationError = Namespaces.Data + nameof(DataValidationError);
        public const string InvisibleToDbDesignerAttribute = Namespaces.Data_Annotations + nameof(InvisibleToDbDesignerAttribute);
        public const string DbTableAttribute = Namespaces.Data_Annotations + nameof(DbTableAttribute);
        public const string ForeignKeyRule = Namespaces.Data + nameof(ForeignKeyRule);
        public const string DbInitializerOf = Namespaces.Data_Primitives + "DbInitializer`1";
        public const string DbSessionProviderOf = Namespaces.Data_DbInit + "DbSessionProvider`1";
        public const string EmptyDbAttribute = Namespaces.Data_DbInit + nameof(EmptyDbAttribute);
        public const string InputAttribute = Namespaces.Data_DbInit + nameof(InputAttribute);
        public const string DataSetOf = Namespaces.Data + "DataSet`1";
        public const string IColumns = Namespaces.Data + nameof(IColumns);
    }
}
