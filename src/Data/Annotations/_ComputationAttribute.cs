using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(ComputationAttribute))]
    public sealed class _ComputationAttribute : Attribute
    {
    }
}
