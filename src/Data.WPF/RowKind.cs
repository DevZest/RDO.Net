using System;

namespace DevZest.Data.Windows
{
    [Flags]
    public enum RowKind
    {
        DataRow = 0,
        EmptySet = 1,
        Eof = 2
    }
}
