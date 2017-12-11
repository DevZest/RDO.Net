using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ColumnWiringAttribute : Attribute
    {
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
