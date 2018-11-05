namespace DevZest.Data.Addons
{
    [Addon(typeof(TempTableIdentity))]
    internal sealed class TempTableIdentity : Identity
    {
        internal static new TempTableIdentity FromInt32Column(_Int32 column, int seed, int increment)
        {
            return new TempTableIdentity(column, seed, increment);
        }

        private TempTableIdentity(Column column, int seed, int increment)
            : base(column, seed, increment)
        {
        }
    }
}
