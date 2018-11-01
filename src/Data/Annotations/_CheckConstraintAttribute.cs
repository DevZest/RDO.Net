using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class _CheckConstraintAttribute : Attribute
    {
    }
}
