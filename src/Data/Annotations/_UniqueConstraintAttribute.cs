using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class _UniqueConstraintAttribute : Attribute
    {
    }
}
