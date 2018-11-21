using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PkColumnAttribute : Attribute
    {
        public PkColumnAttribute(int index = 0)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
