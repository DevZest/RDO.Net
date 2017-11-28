using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ModelMemberAttribute : Attribute
    {
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
