using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CrossReferenceAttribute : Attribute
    {
        public CrossReferenceAttribute(Type type)
        {
            Type = type.VerifyNotNull(nameof(type));
        }

        public Type Type { get; }
    }
}
