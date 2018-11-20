using System;

namespace DevZest.Data.Annotations.Primitives
{
    public sealed class ModelDeclarationSpecAttribute : Attribute
    {
        public ModelDeclarationSpecAttribute(bool isProperty, Type returnType, params Type[] parameterTypes)
        {
            IsProperty = isProperty;
            ReturnType = returnType.VerifyNotNull(nameof(returnType));
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
        }

        public bool IsProperty { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }
    }
}
