using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class JsonIgnoreAttribute : ColumnAttribute
    {
        protected internal override void Initialize(Column column)
        {
            column.JsonIgnore();
        }
    }
}
