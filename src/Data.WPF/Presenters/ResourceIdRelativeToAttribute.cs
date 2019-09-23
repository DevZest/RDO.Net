using System;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Specifies the root namespace to resolve resource by id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ResourceIdRelativeToAttribute : Attribute
    {
        private string _relativeTo;

        /// <summary>
        /// Initializes a new instance of <see cref="ResourceIdRelativeToAttribute"/>.
        /// </summary>
        /// <param name="type">Any type in the root namespace.</param>
        public ResourceIdRelativeToAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _relativeTo = type.Namespace;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ResourceIdRelativeToAttribute"/>.
        /// </summary>
        /// <param name="relativeTo">The root namespace that the resource relative to.</param>
        public ResourceIdRelativeToAttribute(string relativeTo)
        {
            if (relativeTo == null)
                throw new ArgumentNullException(nameof(relativeTo));
            _relativeTo = relativeTo;
        }

        internal static string GetResourceIdRelativeTo(Assembly assembly)
        {
            ResourceIdRelativeToAttribute attr = assembly.GetCustomAttributes<ResourceIdRelativeToAttribute>().FirstOrDefault();
            return attr == null ? string.Empty : attr._relativeTo;
        }
    }
}
