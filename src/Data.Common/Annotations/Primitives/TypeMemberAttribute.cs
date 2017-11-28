using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class TypeMemberAttribute : Attribute
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
            get { return CoerceDeclaringModelTypeOnly(_declaringTypeOnly); }
            set { _declaringTypeOnly = value; }
        }

        protected virtual bool CoerceDeclaringModelTypeOnly(bool value)
        {
            return value;
        }
    }
}
