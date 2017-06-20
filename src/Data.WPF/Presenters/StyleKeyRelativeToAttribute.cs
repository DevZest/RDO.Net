using System;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Presenters
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class StyleKeyRelativeToAttribute : Attribute
    {
        private string _relativeTo;

        public StyleKeyRelativeToAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _relativeTo = type.Namespace;
        }

        public StyleKeyRelativeToAttribute(string relativeTo)
        {
            if (relativeTo == null)
                throw new ArgumentNullException(nameof(relativeTo));
            _relativeTo = relativeTo;
        }

        internal static string GetStyleKeyRelativeTo(Assembly assembly)
        {
            StyleKeyRelativeToAttribute attr = assembly.GetCustomAttributes<StyleKeyRelativeToAttribute>().FirstOrDefault();
            return attr == null ? string.Empty : attr._relativeTo;
        }
    }
}
