using System;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Specifies model level declaration.
    /// </summary>
    public abstract class ModelDeclarationAttribute : ModelAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModelDeclarationAttribute"/>.
        /// </summary>
        /// <param name="name"></param>
        protected ModelDeclarationAttribute(string name)
        {
            name.VerifyNotEmpty(nameof(name));
            Name = name;
        }

        /// <summary>
        /// Gets the name of the model declaration.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the property getter.
        /// </summary>
        /// <param name="returnType">The return type of the property.</param>
        /// <returns>The property getter.</returns>
        protected MethodInfo GetPropertyGetter(Type returnType)
        {
            returnType.VerifyNotNull(nameof(returnType));

            var propertyInfo = GetPropertyInfo(returnType);
            var result = propertyInfo?.GetGetMethod(true);
            if (result == null)
                throw new InvalidOperationException(DiagnosticMessages.NamedModelAttribute_FailedToResolvePropertyGetter(ModelType, Name, returnType));
            return result;
        }

        private PropertyInfo GetPropertyInfo(Type returnType)
        {
            return ModelType.GetProperty(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, returnType, Array.Empty<Type>(), null);
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        /// <param name="paramTypes">Type of parameters of the method.</param>
        /// <param name="returnType">Type of return type of the method.</param>
        /// <returns>The method info.</returns>
        protected MethodInfo GetMethodInfo(Type[] paramTypes, Type returnType)
        {
            paramTypes.VerifyNotNull(nameof(paramTypes));
            returnType.VerifyNotNull(nameof(returnType));
            var result = ModelType.GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            if (result == null || result.ReturnType != returnType)
                throw new InvalidOperationException(DiagnosticMessages.Common_FailedToResolveInstanceMethod(ModelType, Name, string.Join(", ", (object[])paramTypes), returnType));
            return result;
        }
    }
}
