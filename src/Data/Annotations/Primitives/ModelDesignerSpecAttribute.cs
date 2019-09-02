using System;
using System.Collections.Generic;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Defines model designer specification for underlying attribute which can be applied to model member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModelDesignerSpecAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModelDesignerSpecAttribute"/>.
        /// </summary>
        /// <param name="addonTypes">The addon types which will be set by underlying attribute.</param>
        /// <param name="validOnTypes">The types of the model member that the underlying attribute can apply to.</param>
        public ModelDesignerSpecAttribute(Type[] addonTypes, Type[] validOnTypes)
        {
            AddonTypes = addonTypes ?? Array.Empty<Type>();
            ValidOnTypes = validOnTypes ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Gets the addon types which will be set by underlying attribute.
        /// </summary>
        public IReadOnlyList<Type> AddonTypes { get; }

        /// <summary>
        /// Gets the types of the model member that the underlying attribute can apply to.
        /// </summary>
        public IReadOnlyList<Type> ValidOnTypes { get; }

        /// <summary>
        /// Gets a value indicates whether the underlying attribute requires constructor argument.
        /// </summary>
        public bool RequiresArgument { get; set; }
    }
}
