using System;

namespace DevZest.Data.Windows
{
    [Flags]
    public enum DataViewRowType
    {
        DataRow = 1,
        EmptyDataRow = 2,
        Eof = 4
    }
}
