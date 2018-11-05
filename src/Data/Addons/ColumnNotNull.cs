namespace DevZest.Data.Addons
{
    [Addon(typeof(ColumnNotNull))]
    public sealed class ColumnNotNull : IAddon
    {
        public static readonly ColumnNotNull Singleton = new ColumnNotNull();

        private ColumnNotNull()
        {
        }

        public object Key
        {
            get { return typeof(ColumnNotNull); }
        }
    }
}
