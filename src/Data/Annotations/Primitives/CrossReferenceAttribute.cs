using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CrossReferenceAttribute : Attribute
    {
        public CrossReferenceAttribute(Type attibuteType)
        {
            AttributeType = attibuteType.VerifyNotNull(nameof(attibuteType));
        }

        public Type AttributeType { get; }
    }
}
