using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(UniqueConstraintAttribute))]
    public sealed class _UniqueConstraintAttribute : Attribute
    {
    }
}
