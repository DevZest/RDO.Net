namespace DevZest.Data
{
    public sealed class _ValidationMessage : Model
    {
        public static readonly Accessor<_ValidationMessage, _String> DataRowAccessor = RegisterColumn((_ValidationMessage x) => x.DataRow);
        public static readonly Accessor<_ValidationMessage, _String> ValidatorIdAccessor = RegisterColumn((_ValidationMessage x) => x.ValidatorId);
        public static readonly Accessor<_ValidationMessage, _Int32> ValidationLevelAccessor = RegisterColumn((_ValidationMessage x) => x.ValidationLevel);
        public static readonly Accessor<_ValidationMessage, _String> ColumnsAccessor = RegisterColumn((_ValidationMessage x) => x.Columns);
        public static readonly Accessor<_ValidationMessage, _String> DescriptionAccessor = RegisterColumn((_ValidationMessage x) => x.Description);

        public _String DataRow { get; private set; }

        public _String ValidatorId { get; private set; }

        public _Int32 ValidationLevel { get; private set; }

        public new _String Columns { get; private set; }

        public _String Description { get; private set; }
    }
}
