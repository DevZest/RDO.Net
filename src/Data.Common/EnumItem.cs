using System;
using System.Collections.Concurrent;

namespace DevZest.Data
{
    public struct EnumItem<T>
    {
        public EnumItem(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public readonly T Value;
        public readonly string Description;
    }
}
