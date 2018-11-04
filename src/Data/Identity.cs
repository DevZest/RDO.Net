using DevZest.Data.Primitives;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class Identity : IExtension
    {
        internal static readonly string FULL_NAME_TABLE = typeof(Identity).FullName + ".Table";
        internal static readonly string FULL_NAME_TEMP_TABLE = typeof(Identity).FullName + ".TempTable";

        internal static Identity FromInt16Column(_Int16 column, int seed, int increment, bool isTempTable)
        {
            return new Identity(column, seed, increment, isTempTable);
        }

        internal static Identity FromInt32Column(_Int32 column, int seed, int increment, bool isTempTable)
        {
            return new Identity(column, seed, increment, isTempTable);
        }

        internal static Identity FromInt64Column(_Int64 column, int seed, int increment, bool isTempTable)
        {
            return new Identity(column, seed, increment, isTempTable);
        }

        private Identity(Column column, int seed, int increment, bool isTempTable)
        {
            Column = column;
            Seed = seed;
            Increment = increment;
            IsTempTable = isTempTable;
        }

        object IExtension.Key
        {
            get { return IsTempTable ? FULL_NAME_TEMP_TABLE : FULL_NAME_TABLE; }
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

        public int Seed { get; private set; }

        public int Increment { get; private set; }

        public bool IsTempTable { get; private set; }
    }
}
