using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the implementation of unique constraint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(UniqueConstraintAttribute))]
    public sealed class _UniqueConstraintAttribute : ModelImplementationAttribute
    {
    }
}
