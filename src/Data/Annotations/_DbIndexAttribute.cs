using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the implementation of database index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [CrossReference(typeof(DbIndexAttribute))]
    public sealed class _DbIndexAttribute : ModelImplementationAttribute
    {
    }
}
