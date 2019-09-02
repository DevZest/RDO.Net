using System;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Indicates sorting order
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class SortAttribute : Attribute
    {
        /// <summary>Gets the sorting order.</summary>
        protected abstract SortDirection Direction { get; }
    }
}
