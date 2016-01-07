using System;

namespace DevZest.Data.Windows
{
    [Flags]
    public enum RowType
    {
        DataRow = 0,
        EmptySet = 1,
        Eof = 2
    }
}
