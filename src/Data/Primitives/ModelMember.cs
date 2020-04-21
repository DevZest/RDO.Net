using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Base class for member properties which can be registered with <see cref="Model"/> derived class.
    /// </summary>
    public abstract class ModelMember : AddonBag, ISealable
    {
        internal ModelMember()
        {
        }

        internal Model Parent { get; set; }

        /// <summary>
        /// Gets the parent model.
        /// </summary>
        public Model GetParent()
        {
            return Parent;
        }

        /// <summary>Gets the type which declares this member.</summary>
        /// <remarks>The <see cref="DeclaringType"/> can differ from type of <see cref="Parent"/> when the member
        /// is registered in base type of <see cref="Parent"/>.</remarks>
        internal Type DeclaringType { get; set; }

        internal string Name { get; set; }

        internal virtual void ConstructModelMember(Model parent, Type declaringType, string name)
        {
            Parent = parent;
            DeclaringType = declaringType;
            Name = name;
            if (!IsLocal)
                Parent.AddMember(this);
        }

        internal virtual bool IsLocal
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicates whether this <see cref="ModelMember"/> is in design mode.
        /// </summary>
        protected internal virtual bool DesignMode
        {
            get
            {
                var parent = Parent;
                return parent == null ? true : parent.DesignMode;
            }
        }

        /// <summary>
        /// Verifies this<see cref="ModelMember"/> is in design mode.
        /// </summary>
        /// <remarks>
        /// Inheritors should call this method at beginning of 
        /// any API that writes to data members should be freezed when <see cref="DesignMode"/> is false.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The <see cref="DesignMode"/> is false and cannot have its members written to.</exception>
        protected void VerifyDesignMode()
        {
            if (!DesignMode)
                throw new InvalidOperationException(DiagnosticMessages.Common_VerifyDesignMode);
        }

        /// <inheritdoc/>
        bool ISealable.IsSealed
        {
            get { return !DesignMode; }
        }
    }
}
