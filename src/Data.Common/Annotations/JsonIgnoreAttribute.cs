using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class JsonIgnoreAttribute : ColumnAttribute
    {
        public JsonIgnoreAttribute()
        {
            DeclaringModelTypeOnly = true;
        }

        protected override void Initialize(Column column)
        {
            column.JsonIgnore();
        }
    }
}
