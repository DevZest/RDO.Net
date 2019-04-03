using System;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Presenters
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ResourceIdRelativeToAttribute : Attribute
    {
        private string _relativeTo;

        public ResourceIdRelativeToAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _relativeTo = type.Namespace;
        }

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
