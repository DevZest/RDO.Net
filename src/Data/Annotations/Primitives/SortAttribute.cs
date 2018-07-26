using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class SortAttribute : Attribute
    {
        protected abstract SortDirection Direction { get; }
    }
}
