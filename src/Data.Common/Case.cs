using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static class Case
    {
        public static CaseExpression<T> WhenThen<T>(_Boolean when, Column<T> then)
        {
            return new CaseExpression<T>().WhenThen(when, then);
        }
    }
}
