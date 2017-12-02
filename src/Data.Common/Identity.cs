using DevZest.Data.Primitives;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class Identity : IResource
    {
        internal static readonly string FULL_NAME_TABLE = typeof(Identity).FullName + ".Table";
        internal static readonly string FULL_NAME_TEMP_TABLE = typeof(Identity).FullName + ".TempTable";

        internal Identity(_Int32 column, int seed, int increment, bool isTempTable)
        {
            Debug.Assert(!object.ReferenceEquals(column, null));
            Debug.Assert(increment != 0);
            Column = column;
            Seed = seed;
            Increment = increment;
            IsTempTable = isTempTable;
        }

        object IResource.Key
        {
            get { return IsTempTable ? FULL_NAME_TEMP_TABLE : FULL_NAME_TABLE; }
        }

        public _Int32 Column { get; private set; }

        public int Seed { get; private set; }

        public int Increment { get; private set; }

        public bool IsTempTable { get; private set; }
    }
}
