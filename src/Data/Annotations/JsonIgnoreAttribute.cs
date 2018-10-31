using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class JsonIgnoreAttribute : ColumnAttribute
    {
        public JsonIgnoreAttribute()
        {
            DeclaringTypeOnly = true;
        }

        protected override void Wireup(Column column)
        {
            column.JsonIgnore();
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
