namespace DevZest.Data.Primitives
{
    public sealed class ColumnNotNull : IExtension
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
