using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies that property is check constraint implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(CheckConstraintAttribute))]
    public sealed class _CheckConstraintAttribute : ModelImplementationAttribute
    {
    }
}
