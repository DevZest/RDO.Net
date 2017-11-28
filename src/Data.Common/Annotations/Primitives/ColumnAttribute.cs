using System;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>Base class for attributes which can be decorated with a column.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ColumnAttribute : TypeMemberAttribute
    {
        internal void TryInitialize(Column column)
        {
            if (DeclaringTypeOnly && column.ParentModel.GetType() != DeclaringType)
                return;
            Initialize(column);
        }

        /// <summary>Initializes the provided <see cref="Column"/> object.</summary>
        /// <param name="column">The <see cref="Column"/> object.</param>
        protected abstract void Initialize(Column column);
    }
}
