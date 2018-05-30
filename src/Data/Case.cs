using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static class Case
    {
        public static CaseWhen When(_Boolean when)
        {
            return new CaseWhen(when);
        }

        public static CaseOn<T> On<T>(Column<T> on)
        {
            on.VerifyNotNull(nameof(on));
            return new CaseOn<T>(on);
        }
    }
}
