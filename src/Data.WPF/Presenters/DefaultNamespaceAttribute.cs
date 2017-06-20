using System;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Presenters
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class DefaultNamespaceAttribute : Attribute
    {
        private string _defaultNamespace;

        public DefaultNamespaceAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _defaultNamespace = type.Namespace;
        }

        public DefaultNamespaceAttribute(string defaultNamespace)
        {
            if (defaultNamespace == null)
                throw new ArgumentNullException(nameof(defaultNamespace));
            _defaultNamespace = defaultNamespace;
        }

        internal static string GetDefaultNamespace(Assembly assembly)
        {
            DefaultNamespaceAttribute attr = assembly.GetCustomAttributes<DefaultNamespaceAttribute>().FirstOrDefault();
            return attr == null ? string.Empty : attr._defaultNamespace;
        }
    }
}
