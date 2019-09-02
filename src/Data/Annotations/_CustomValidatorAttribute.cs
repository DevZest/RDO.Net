using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the implementation of custom validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(CustomValidatorAttribute))]
    public sealed class _CustomValidatorAttribute : ModelImplementationAttribute
    {
    }
}
