using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DefaultConstraint<T> : DefaultConstraint
    {
        internal DefaultConstraint(Column<T> defaultValue, string name)
            : base(name)
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
