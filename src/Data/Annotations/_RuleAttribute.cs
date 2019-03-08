using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(RuleAttribute))]
    public sealed class _RuleAttribute : ModelImplementationAttribute
    {
    }
}
