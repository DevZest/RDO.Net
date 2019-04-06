using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies that method is column computation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(ComputationAttribute))]
    public sealed class _ComputationAttribute : ModelImplementationAttribute
    {
    }
}
