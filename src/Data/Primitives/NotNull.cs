namespace DevZest.Data.Primitives
{
    public sealed class NotNull : IExtension
    {
        public static readonly NotNull Singleton = new NotNull();

        private NotNull()
        {
        }

        public object Key
        {
            get { return typeof(NotNull); }
        }
    }
}
