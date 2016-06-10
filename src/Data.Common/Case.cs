using DevZest.Data.Primitives;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public static class Case
    {
        public static CaseExpression<T> WhenThen<T>(_Boolean when, Column<T> then)
        {
            return new CaseExpression<T>().WhenThen(when, then);
        }

        public static CaseOnExpressionBuilder<T> On<T>(Column<T> on)
        {
            Check.NotNull(on, nameof(on));
            return new CaseOnExpressionBuilder<T>(on);
        }
    }
}
