using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Indicates property's value is provided from user input.
    /// </summary>
    /// <remarks>Apply this attribute on public settable string property, for sensitive data such as password.
    /// <see cref="DbInitExtensions.RunDbInit(string[])"/> will inject value for the property, by first looking
    /// from environment variable, then read from console if environment variable does not exist.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InputAttribute : Attribute
    {
        /// <summary>
        /// Gets the property associated with this attribute.
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Resolves declared <see cref="InputAttribute"/> attributes for specified type.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <returns>The declared <see cref="InputAttribute"/> attributes.</returns>
        public static InputAttribute[] Resolve(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return type.GetProperties().Select(x => GetInputAttribute(x)).Where(x => x != null).OrderBy(x => x.Order).ToArray();
        }

        internal void SetValue(object obj, string value)
        {
            Debug.Assert(Property != null);
            Property.SetValue(obj, value);
        }

        private static InputAttribute GetInputAttribute(PropertyInfo propertyInfo)
        {
            if (!IsPublicStringProperty(propertyInfo))
                return null;

            var result = propertyInfo.GetCustomAttribute<InputAttribute>();
            if (result == null)
                return null;

            result.Property = propertyInfo;
            return result;
        }

        private static bool IsPublicStringProperty(PropertyInfo propertyInfo)
        {
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return false;

            if (setMethod.IsStatic)
                return false;

            if (!setMethod.IsPublic)
                return false;

            var parameters = setMethod.GetParameters();
            if (parameters == null || parameters.Length != 1)
                return false;

            var parameter = parameters[0];
            if (parameter.ParameterType != typeof(string))
                return false;

            return true;
        }

        private string PropertyName
        {
            get { return Property?.Name; }
        }

        private string _title;
        /// <summary>
        /// Gets or sets the input title of the property.
        /// </summary>
        /// <remarks>If not set, returns the name of the property.</remarks>
        public string Title
        {
            get { return _title ?? PropertyName; }
            set { _title = value; }
        }

        private string _environmentVariableName;
        /// <summary>
        /// Gets or sets the environment variable name of the property.
        /// </summary>
        /// <remarks>If not set, returns the name of the property.</remarks>
        public string EnvironmentVariableName
        {
            get { return _environmentVariableName ?? PropertyName; }
            set { _environmentVariableName = value; }
        }

        /// <summary>
        /// Gets or sets value indicates whether this property is password.
        /// </summary>
        /// <remarks>Set <see cref="IsPassword"/> to <see langword="true"/> to avoid displaying the password when input from user.</remarks>
        public bool IsPassword { get; set; }

        /// <summary>
        /// Gets or sets the order to input if multiple input property exists.
        /// </summary>
        public int Order { get; set; }
    }
}
