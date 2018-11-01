using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(CheckConstraintAttribute))]
    public sealed class _CheckConstraintAttribute : Attribute
    {
    }
}
