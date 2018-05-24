using System;

namespace DevZest.Data
{
    public enum JsonValueType
    {
        // These values are used by JsonParser.TokenKind, which has a FlagsAttribute
        Number = 0x1,
        String = 0x2,
        True = 0x4,
        False = 0x8,
        Null = 0x10
    }
}
