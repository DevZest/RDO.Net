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
            if (IsSolitary && column.ParentModel.GetType() != ModelType)
                return;
            Initialize(column);
        }

        /// <summary>Initializes the provided <see cref="Column"/> object.</summary>
        /// <param name="column">The <see cref="Column"/> object.</param>
        protected abstract void Initialize(Column column);

        private Type _modelType;
        public Type ModelType
        {
            get { return _modelType; }
            internal set
            {
                Debug.Assert(_modelType == null && value != null);
                _modelType = value;
            }
        }

        private bool _isSolitary;
        public bool IsSolitary
        {
            get { return CoerceIsSolitary(_isSolitary); }
            set { _isSolitary = value; }
        }

        protected virtual bool CoerceIsSolitary(bool value)
        {
            return value;
        }
    }
}
