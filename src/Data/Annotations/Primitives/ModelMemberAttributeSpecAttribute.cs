using System;
using System.Collections.Generic;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModelMemberAttributeSpecAttribute : Attribute
    {
        public ModelMemberAttributeSpecAttribute(Type[] addonTypes, Type[] validOnTypes)
        {
            AddonTypes = addonTypes ?? Array.Empty<Type>();
            ValidOnTypes = validOnTypes ?? Array.Empty<Type>();
        }

        public IReadOnlyList<Type> AddonTypes { get; }

        public IReadOnlyList<Type> ValidOnTypes { get; }

        public bool RequiresArgument { get; set; }
    }
}
