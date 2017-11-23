using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class UniqueAttribute : ValidatorColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            base.Initialize(column);
            column.ParentModel.Unique(Name, IsCluster, SortDirection == SortDirection.Descending ? column.Desc() : column.Asc());
        }

        public string Name { get; set; }

        public bool IsCluster { get; set; }

        public SortDirection SortDirection { get; set; }

        protected override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(MessageId, ValidationSeverity.Error, GetMessage(column, dataRow), column);
        }

        private bool IsValid(Column column, DataRow dataRow)
        {
            var dataSet = column.ParentModel.DataSet;
            foreach (var other in dataSet)
            {
                if (other == dataRow)
                    continue;
                if (column.Compare(dataRow, other, SortDirection) == 0)
                    return false;
            }
            return true;
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            return Strings.UniqueAttribute_DefaultErrorMessage(column);
        }
    }
}
