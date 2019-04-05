namespace DevZest.Data.Addons
{
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

        public Column Column { get; }

        public _Int16 Int16Column
        {
            get { return Column as _Int16; }
        }

        public _Int32 Int32Column
        {
            get { return Column as _Int32; }
        }

        public _Int64 Int64Column
        {
            get { return Column as _Int64; }
        }

        public long Seed { get; private set; }

        public long Increment { get; private set; }
    }
}
