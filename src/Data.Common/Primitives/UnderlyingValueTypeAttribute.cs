using System;

namespace DevZest.Data.Primitives
{
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false)]
    public sealed class UnderlyingValueTypeAttribute : Attribute
    {
    }
}
