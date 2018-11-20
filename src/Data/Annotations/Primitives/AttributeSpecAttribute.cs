using System;
using System.Collections.Generic;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AttributeSpecAttribute : Attribute
    {
        public AttributeSpecAttribute(Type[] addonTypes, Type[] validOnTypes)
        {
            ValidOnTypes = validOnTypes ?? Array.Empty<Type>();
            AddonTypes = addonTypes ?? Array.Empty<Type>();
        }

        public IReadOnlyList<Type> AddonTypes { get; }

        public IReadOnlyList<Type> ValidOnTypes { get; }

        public bool RequiresArgument { get; set; }
    }
}
