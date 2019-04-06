namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents the identity (auto increment) metadata of <see cref="Column"/>.
    /// </summary>
    public class Identity : IAddon
    {
        internal static Identity FromInt16Column(_Int16 column, short seed, short increment)
        {
            return new Identity(column, seed, increment);
        }

        internal static Identity FromInt32Column(_Int32 column, int seed, int increment)
        {
            return new Identity(column, seed, increment);
        }

        internal static Identity FromInt64Column(_Int64 column, long seed, long increment)
        {
            return new Identity(column, seed, increment);
        }

        internal Identity(Column column, long seed, long increment)
        {
            Column = column;
            Seed = seed;
            Increment = increment;
        }

        object IAddon.Key
        {
            get { return GetType(); }
        }

        /// <summary>
        /// Gets the column owner for this identity.
        /// </summary>
        public Column Column { get; }

        /// <summary>
        /// Gets the <see cref="_Int16"/> column owner for this identity.
        /// </summary>
        public _Int16 Int16Column
        {
            get { return Column as _Int16; }
        }

        /// <summary>
        /// Gets the <see cref="_Int32"/> column owner for this identity.
        /// </summary>
        public _Int32 Int32Column
        {
            get { return Column as _Int32; }
        }

        /// <summary>
        /// Gets the <see cref="_Int64"/> column owner for this identity.
        /// </summary>
        public _Int64 Int64Column
        {
            get { return Column as _Int64; }
        }

        /// <summary>
        /// Gets the value for the column that is used for the very first row loaded into the table.
        /// </summary>
        public long Seed { get; private set; }

        /// <summary>
        /// Gets the incremental value that is added to the identity value of the previous row that was loaded.
        /// </summary>
        public long Increment { get; private set; }
    }
}
