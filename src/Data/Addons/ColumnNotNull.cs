namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents column not null metadata.
    /// </summary>
    /// <remarks><see cref="ColumnNotNull"/> implements <see cref="IAddon"/> with key of <see cref="ColumnNotNull"/> type.</remarks>
    public sealed class ColumnNotNull : IAddon
    {
        /// <summary>
        /// The singleton instance of <see cref="ColumnNotNull"/> class.
        /// </summary>
        public static readonly ColumnNotNull Singleton = new ColumnNotNull();

        private ColumnNotNull()
        {
        }

        /// <summary>
        /// Gets the key of <see cref="IAddon"/>.
        /// </summary>
        public object Key
        {
            get { return typeof(ColumnNotNull); }
        }
    }
}
