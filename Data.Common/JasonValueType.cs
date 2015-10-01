using System;

namespace DevZest.Data
{
    [Flags]
    public enum JsonValueType
    {
        Number = 0x1,
        String = 0x2,
        True = 0x4,
        False = 0x8,
        Null = 0x10
    }
}
