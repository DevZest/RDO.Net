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
            public const string Data_Annotations = Data + "Annotations.";
            public const string Data_Annotations_Primitives = Data_Annotations + "Primitives.";
        }

        public const string AttributeUsageAttribute = Namespaces.System + nameof(AttributeUsageAttribute);

        public const string Model = Namespaces.Data + nameof(Model);
        public const string Column = Namespaces.Data + nameof(Column);
        public const string LocalColumn = Namespaces.Data + nameof(LocalColumn) + "`1";
        public const string ColumnList = Namespaces.Data + nameof(ColumnList);
        public const string Projection = Namespaces.Data + nameof(Projection);
        public const string PrimaryKey = Namespaces.Data + nameof(PrimaryKey);
        public const string AscAttribute = Namespaces.Data_Annotations + nameof(AscAttribute);
        public const string DescAttribute = Namespaces.Data_Annotations + nameof(DescAttribute);
        public const string PkColumnAttribute = Namespaces.Data_Annotations + nameof(PkColumnAttribute);
        public const string PropertyRegistrationAttribute = Namespaces.Data_Annotations_Primitives + nameof(PropertyRegistrationAttribute);
        public const string CreateKeyAttribute = Namespaces.Data_Annotations_Primitives + nameof(CreateKeyAttribute);
        public const string ModelDeclarationAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelDeclarationAttribute);
        public const string ModelImplementationAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelImplementationAttribute);
        public const string CrossReferenceAttribute = Namespaces.Data_Annotations_Primitives + nameof(CrossReferenceAttribute);
        public const string ModelDeclarationSpecAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelDeclarationSpecAttribute);

        public const string ModelMemberAttributeSpecAttribute = Namespaces.Data_Annotations_Primitives + nameof(ModelMemberAttributeSpecAttribute);
        public const string AddonAttribute = Namespaces.Data_Addons + nameof(AddonAttribute);
    }
}
