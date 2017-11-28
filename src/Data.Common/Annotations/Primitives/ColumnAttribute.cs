using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>Base class for attributes which can be decorated with a column.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public abstract class ColumnAttribute : Attribute
    {
        internal void TryInitialize(Column column)
        {
            if (DeclaringModelTypeOnly && column.ParentModel.GetType() != DeclaringModelType)
                return;
            Initialize(column);
        }

        /// <summary>Initializes the provided <see cref="Column"/> object.</summary>
        /// <param name="column">The <see cref="Column"/> object.</param>
        protected abstract void Initialize(Column column);

        private Type _declaringModelType;
        public Type DeclaringModelType
        {
            get { return _declaringModelType; }
            internal set
            {
                Debug.Assert(_declaringModelType == null && value != null);
                _declaringModelType = value;
            }
        }

        private bool _declaringModelTypeOnly;
        public bool DeclaringModelTypeOnly
        {
            get { return CoerceDeclaringModelTypeOnly(_declaringModelTypeOnly); }
            set { _declaringModelTypeOnly = value; }
        }

        protected virtual bool CoerceDeclaringModelTypeOnly(bool value)
        {
            return value;
        }
    }
}
