using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(ValidatorAttribute))]
    public sealed class _ValidatorAttribute : Attribute
    {
    }
}
