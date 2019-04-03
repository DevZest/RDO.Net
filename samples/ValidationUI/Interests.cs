using System;

namespace ValidationUI
{
    [Flags]
    public enum Interests
    {
        None = 0,
        Music = 1,
        Movies = 2,
        Sports = 4,
        Shopping = 8,
        Hunting = 16,
        Books = 32,
        Physics = 64,
        Comics = 128
    }
}
