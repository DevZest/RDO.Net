using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [CrossReference(typeof(DbIndexAttribute))]
    public sealed class _DbIndexAttribute : Attribute
    {
    }
}
