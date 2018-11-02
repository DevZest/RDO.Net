using System;

namespace DevZest.Data.Annotations.Primitives
{
    public sealed class ImplementationAttribute : Attribute
    {
        public ImplementationAttribute(ImplementationKind kind, Type returnType, params Type[] parameterTypes)
        {
            Kind = kind;
            ReturnType = returnType.VerifyNotNull(nameof(returnType));
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
        }

        public ImplementationKind Kind { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }
    }
}
