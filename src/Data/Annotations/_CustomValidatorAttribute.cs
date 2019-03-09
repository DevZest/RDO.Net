using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(CustomValidatorAttribute))]
    public sealed class _CustomValidatorAttribute : ModelImplementationAttribute
    {
    }
}
