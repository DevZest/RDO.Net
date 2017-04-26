namespace DevZest.Data
{
    public sealed class JsonIgnoreAttribute : ColumnAttribute
    {
        protected internal override void Initialize(Column column)
        {
            column.JsonIgnore();
        }
    }
}
