using System;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Defines implementation specification for model declaration attribute.
    /// </summary>
    /// <remarks>Design-time tools use this attribute to generate the implementation property/method.</remarks>
    public sealed class ModelDeclarationSpecAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModelDeclarationSpecAttribute"/>.
        /// </summary>
        /// <param name="isProperty">Specifies whether the implementation is property or method.</param>
        /// <param name="returnType">Return type of the implementation property/method.</param>
        /// <param name="parameterTypes">Parameter types of the implementation method.</param>
        public ModelDeclarationSpecAttribute(bool isProperty, Type returnType, params Type[] parameterTypes)
        {
            IsProperty = isProperty;
            ReturnType = returnType.VerifyNotNull(nameof(returnType));
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Gets a value indicates whether the implementation is property or method.
        /// </summary>
        public bool IsProperty { get; }

        /// <summary>
        /// Gets the return type of the implementation property/method.
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the parameter types of the implementation method.
        /// </summary>
        public Type[] ParameterTypes { get; }
    }
}
