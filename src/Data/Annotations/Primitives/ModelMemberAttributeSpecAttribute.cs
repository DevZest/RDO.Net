using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModelMemberAttributeSpecAttribute : Attribute
    {
        public ModelMemberAttributeSpecAttribute(object extensionKey, bool isExclusive, params Type[] types)
        {
            ExtensionKey = extensionKey;
            IsExclusive = isExclusive;
            Types = types ?? Array.Empty<Type>();
        }

        public object ExtensionKey { get; }

        public bool IsExclusive { get; }

        public Type[] Types { get; }
    }
}
