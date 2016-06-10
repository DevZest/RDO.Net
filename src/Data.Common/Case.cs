using DevZest.Data.Primitives;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public static class Case
    {
        public static CaseWhen When(_Boolean when)
        {
            return new CaseWhen(when);
        }

        public static CaseOnExpressionBuilder<T> On<T>(Column<T> on)
        {
            Check.NotNull(on, nameof(on));
            return new CaseOnExpressionBuilder<T>(on);
        }
    }
}
