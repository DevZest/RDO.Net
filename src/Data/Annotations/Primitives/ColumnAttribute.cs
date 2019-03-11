using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>Base class for attributes which can be decorated with a column.</summary>
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
        public bool DeclaringTypeOnly
        {
            get { return CoerceDeclaringTypeOnly(_declaringTypeOnly); }
            set { _declaringTypeOnly = value; }
        }

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
