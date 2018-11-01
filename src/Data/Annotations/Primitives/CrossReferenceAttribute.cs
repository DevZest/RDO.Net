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

        public CrossReferenceAttribute(Type attributeType, Type returnType, params Type[] parameterTypes)
            : this(attributeType)
        {
            ReturnType = returnType.VerifyNotNull(nameof(returnType));
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
        }

        public Type AttributeType { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }
    }
}
