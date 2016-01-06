namespace DevZest.Data
{
    public sealed class IgnoreColumnAttribute : ColumnAttribute
    {
        protected internal override void Initialize(Column column)
        {
            column.IgnoreColumn();
        }
    }
}
