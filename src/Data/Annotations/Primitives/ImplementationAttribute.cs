using System;

namespace DevZest.Data.Annotations.Primitives
{
    public sealed class ImplementationAttribute : Attribute
    {
        public ImplementationAttribute(Type type)
        {
            Type = type.VerifyNotNull(nameof(type));
        }

        public Type Type { get; }
    }
}
