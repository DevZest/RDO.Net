using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class Default<T> : Default
    {
        internal Default(Column<T> defaultValue)
        {
            Debug.Assert(defaultValue != null && defaultValue.IsExpression);
            _defaultValue = defaultValue;
        }

        private Column<T> _defaultValue;
        public T Value
        {
            get { return _defaultValue[null]; }
        }

        public override DbExpression DbExpression
        {
            get { return _defaultValue.DbExpression; }
        }
    }
}
