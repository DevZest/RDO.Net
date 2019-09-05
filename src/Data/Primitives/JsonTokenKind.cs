using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Specifies the kind of JSON token.
    /// </summary>
    [Flags]
    public enum JsonTokenKind
    {
        /// <summary>
        /// Token is Eof.
        /// </summary>
        Eof = 0,

        /// <summary>
        /// Token is string.
        /// </summary>
        String = JsonValueType.String,

        /// <summary>
        /// Token is number.
        /// </summary>
        Number = JsonValueType.Number,

        /// <summary>
        /// Token is 'true' literal.
        /// </summary>
        True = JsonValueType.True,

        /// <summary>
        /// Token is 'false' literal.
        /// </summary>
        False = JsonValueType.False,

        /// <summary>
        /// Token is 'null' literal.
        /// </summary>
        Null = JsonValueType.Null,

        /// <summary>
        /// Token is curly open '{'.
        /// </summary>
        CurlyOpen = 0x20,

        /// <summary>
        /// Token is curly close '}'.
        /// </summary>
        CurlyClose = 0x40,

        /// <summary>
        /// Token is squared open '['.
        /// </summary>
        SquaredOpen = 0x80,

        /// <summary>
        /// Token is squared close ']'.
        /// </summary>
        SquaredClose = 0x100,

        /// <summary>
        /// Token is property name.
        /// </summary>
        PropertyName = 0x200,

        /// <summary>
        /// Token is comma ','.
        /// </summary>
        Comma = 0x400,

        /// <summary>
        /// Token is column values: <see cref="String"/>, <see cref="Number"/>, <see cref="True"/>, <see cref="False"/> and <see cref="Null"/>.
        /// </summary>
        ColumnValues = String | Number | True | False | Null
    }
}
