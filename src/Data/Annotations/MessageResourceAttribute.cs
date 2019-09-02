using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the resource type for localized messages.
    /// </summary>
    /// <remarks>This attribute will be used by design-time tools to retrieve name of localized messages.
    /// Decorate this attribute with with your assembly.</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MessageResourceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MessageResourceAttribute"/>.
        /// </summary>
        /// <param name="type">The resource type for localized messages.</param>
        public MessageResourceAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the resource type of the localized messages.
        /// </summary>
        public Type Type { get; }
    }
}
