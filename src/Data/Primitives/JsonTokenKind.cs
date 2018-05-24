using System;

namespace DevZest.Data.Primitives
{
    [Flags]
    public enum JsonTokenKind
    {
        Eof = 0,
        String = JsonValueType.String,
        Number = JsonValueType.Number,
        True = JsonValueType.True,
        False = JsonValueType.False,
        Null = JsonValueType.Null,
        CurlyOpen = 0x20,
        CurlyClose = 0x40,
        SquaredOpen = 0x80,
        SquaredClose = 0x100,
        Colon = 0x200,
        Comma = 0x400,
        ColumnValues = String | Number | True | False | Null
    }
}
