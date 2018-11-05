using System;

namespace DevZest.Data.Addons
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AddonAttribute : Attribute
    {
        public AddonAttribute()
        {
        }

        public AddonAttribute(Type typeKey)
        {
            TypeKey.VerifyNotNull(nameof(typeKey));
        }

        public Type TypeKey { get; }
    }
}
