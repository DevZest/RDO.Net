using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>Base class for attributes which can be specified for a column.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ColumnAttribute : Attribute
    {
        internal void TryWireup(Column column)
        {
            if (VerifyDeclaringType(column))
            {
                if (this is ILogicalDataTypeAttribute logicalDataTypeAttribute)
                    column.LogicalDataType = logicalDataTypeAttribute.LogicalDataType;
                Wireup(column);
            }
        }

        /// <summary>Initializes the provided <see cref="Column"/> object.</summary>
        /// <param name="column">The <see cref="Column"/> object.</param>
        protected abstract void Wireup(Column column);

        private Type _declaringType;
        /// <summary>
        /// Gets the type which declares the column property.
        /// </summary>
        public Type DeclaringType
        {
            get { return _declaringType; }
            internal set
            {
                Debug.Assert(_declaringType == null && value != null);
                _declaringType = value;
            }
        }

        private bool _declaringTypeOnly;
        /// <summary>
        /// Gets or sets a value indicates whether this attribute affects the type which declares the columns property only.
        /// </summary>
        /// <remarks>The default value is <see langword="false" />. If set to <see langword="true"/>, it will also affect derived class.</remarks>
        public bool DeclaringTypeOnly
        {
            get { return CoerceDeclaringTypeOnly(_declaringTypeOnly); }
            set { _declaringTypeOnly = value; }
        }

        /// <summary>
        /// Coerce the value of <see cref="DeclaringTypeOnly"/> property.
        /// </summary>
        /// <param name="value">The original <see cref="DeclaringTypeOnly"/> property value.</param>
        /// <returns>The coerced <see cref="DeclaringTypeOnly"/> property value.</returns>
        /// <remarks>Derived class can override this method to enforce <see cref="DeclaringTypeOnly"/> property value.
        /// The default implementation does not change the original value.</remarks>
        protected virtual bool CoerceDeclaringTypeOnly(bool value)
        {
            return value;
        }

        internal bool VerifyDeclaringType(Column column)
        {
            return VerifyDeclaringType(column.DeclaringType);
        }

        private bool VerifyDeclaringType(Type declaringType)
        {
            return !DeclaringTypeOnly || DeclaringType == declaringType;
        }
    }
}
