namespace DevZest.Data.CodeAnalysis
{
    static partial class KnownTypes
    {
        private static partial class Namespaces
        {
            // Must end with "."
            public const string System = "System.";
            public const string Data_Addons = Data + "Addons.";
            public const string Data = "DevZest.Data.";
            public const string Data_Primitives = Data + "Primitives.";
            public const string Data_Annotations = Data + "Annotations.";
            public const string Data_Annotations_Primitives = Data_Annotations + "Primitives.";
        }

        public const string AttributeUsageAttribute = Namespaces.System + nameof(AttributeUsageAttribute);

        public const string Model = Namespaces.Data + nameof(Model);
        public const string GenericModel = Namespaces.Data + "Model`1";
        public const string Column = Namespaces.Data + nameof(Column);
        public const string LocalColumn = Namespaces.Data + nameof(LocalColumn) + "`1";
        public const string ColumnList = Namespaces.Data + nameof(ColumnList);
        public const string Projection = Namespaces.Data + nameof(Projection);
        public const string CandidateKey = Namespaces.Data + nameof(CandidateKey);
        public const string AscAttribute = Namespaces.Data_Annotations + nameof(AscAttribute);
        public const string DescAttribute = Namespaces.Data_Annotations + nameof(DescAttribute);
        public const string PropertyRegistrationAttribute = Namespaces.Data_Annotations_Primitives + nameof(PropertyRegistrationAttribute);
        public const string CreateKeyAttribute = Namespaces.Data_Annotations_Primitives + nameof(CreateKeyAttribute);
        public const string ModelDeclarationAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelDeclarationAttribute);
        public const string ModelImplementationAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelImplementationAttribute);
        public const string CrossReferenceAttribute = Namespaces.Data_Annotations_Primitives + nameof(CrossReferenceAttribute);
        public const string ModelDeclarationSpecAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelDeclarationSpecAttribute);

        public const string ModelDesignerSpecAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelDesignerSpecAttribute);
        public const string AddonAttribute = Namespaces.Data_Addons + nameof(AddonAttribute);

        public const string DbSession = Namespaces.Data_Primitives + nameof(DbSession);
        public const string DbTable = Namespaces.Data + nameof(DbTable) + "`1";
        public const string ForeignKeyAttribute = Namespaces.Data_Annotations + nameof(ForeignKeyAttribute);
        public const string _ForeignKeyAttribute = Namespaces.Data_Annotations + nameof(_ForeignKeyAttribute);
        public const string KeyMapping = Namespaces.Data + nameof(KeyMapping);
    }
}
