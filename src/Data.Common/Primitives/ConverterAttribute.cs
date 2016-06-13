using System;

namespace DevZest.Data.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public abstract class ConverterAttribute : Attribute
    {
        public string TypeId { get; private set; }

        internal virtual void Initialize(Type targetType)
        {
            if (string.IsNullOrEmpty(TypeId))
                TypeId = GetDefaultTypeId(targetType);
        }

        internal virtual string GetDefaultTypeId(Type targetType)
        {
            var fullName = targetType.FullName;
            var nspace = targetType.Namespace;
            return string.IsNullOrEmpty(nspace) ? fullName : fullName.Substring(nspace.Length + 1);
        }
    }
}
