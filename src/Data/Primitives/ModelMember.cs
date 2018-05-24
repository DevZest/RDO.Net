using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Base class for member properties which can be registered with <see cref="Model"/> derived class.
    /// </summary>
    public abstract class ModelMember : ExtensibleObject, IDesignable
    {
        internal ModelMember()
        {
        }

        /// <summary>Gets the parent model of this member. </summary>
        internal Model ParentModel { get; set; }

        public Model GetParent()
        {
            return ParentModel;
        }

        /// <summary>Gets the type which declares this member.</summary>
        /// <remarks>The <see cref="DeclaringType"/> can differ from type of <see cref="ParentModel"/> when the member
        /// is registered as attached.</remarks>
        internal Type DeclaringType { get; set; }

        /// <summary>Gets the name of the member.</summary>
        internal string Name { get; set; }

        internal virtual void ConstructModelMember(Model parentModel, Type declaringType, string name)
        {
            ParentModel = parentModel;
            DeclaringType = declaringType;
            Name = name;
            ParentModel.AddMember(this);
        }

        /// <summary>Gets a value indicates whether this <see cref="ModelMember"/> is in design mode.</summary>
        protected internal virtual bool DesignMode
        {
            get
            {
                var parentModel = ParentModel;
                return parentModel == null ? true : parentModel.DesignMode;
            }
        }

        /// <summary>Verifies this<see cref="ModelMember"/> is in design mode. Inheritors should call this method at beginning of 
        /// any API that writes to data members should be freezed when <see cref="DesignMode"/> is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">The <see cref="DesignMode"/> is false and cannot have its members written to.</exception>
        protected void VerifyDesignMode()
        {
            if (!DesignMode)
                throw new InvalidOperationException(DiagnosticMessages.VerifyDesignMode);
        }

        /// <inheritdoc/>
        bool IDesignable.DesignMode
        {
            get { return DesignMode; }
        }
    }
}
