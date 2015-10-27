using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class Default<T> : Default
    {
        internal Default(Column<T> defaultValue)
        {
            Debug.Assert(defaultValue != null);
            Debug.Assert(defaultValue.IsExpression);
            DefaultValue = defaultValue;
        }

        public Column<T> DefaultValue { get; private set; }

        public override DbExpression DefaultValueExpression
        {
            get { return DefaultValue.DbExpression; }
        }
    }
}
