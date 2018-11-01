using System;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Attribute for <see cref="Model"/> derived class, with name to specify the implementation as member of the class.
    /// </summary>
    public abstract class NamedModelAttribute : ModelAttribute
    {
        protected NamedModelAttribute(string name)
        {
            name.VerifyNotEmpty(nameof(name));
            Name = name;
        }

        public string Name { get; }

        public string Description { get; set; }

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

        protected MethodInfo GetMethodInfo(Type[] paramTypes, Type returnType)
        {
            paramTypes.VerifyNotNull(nameof(paramTypes));
            returnType.VerifyNotNull(nameof(returnType));
            var result = ModelType.GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            if (result == null || result.ReturnType != returnType)
                throw new InvalidOperationException(DiagnosticMessages.NamedModelAttribute_FailedToResolveMethod(ModelType, Name, string.Join(", ", (object[])paramTypes), returnType));
            return result;
        }
    }
}
