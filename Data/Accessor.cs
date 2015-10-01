using System;
using System.Threading;

namespace DevZest.Data
{
    /// <summary>Represents a property accessor registered with a class.</summary>
    /// <typeparam name="TParent">The type of the parent.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <threadsafety static="true" instance="true"/>
    public abstract class Accessor<TParent, TProperty>
    {
        internal Accessor()
        {
        }

        internal abstract Func<TParent, TProperty> Getter { get; }

        internal abstract Action<TParent, TProperty> Setter { get; }

        internal abstract Func<TProperty> Constructor { get; }

        internal abstract Action<TProperty> Initializer { get; }

        Type _originalOwnerType;
        internal Type OriginalOwnerType
        {
            get { return _originalOwnerType ?? OwnerType; }
            set { _originalOwnerType = value; }
        }

        string _originalName;
        internal string OriginalName
        {
            get { return _originalName ?? Name; }
            set { _originalName = value; }
        }

        /// <summary>Gets the type which declares this accessor.</summary>
        public abstract Type OwnerType { get; }

        /// <summary>Gets the name of the property.</summary>
        public abstract string Name { get; }

        /// <summary>Gets the type which the property is member of.</summary>
        public Type ParentType
        {
            get { return typeof(TParent); }
        }

        /// <summary>Gets the type of the property.</summary>
        public Type PropertyType
        {
            get { return typeof(TProperty); }
        }

        static ThreadLocal<TParent> s_parent = new ThreadLocal<TParent>(() => default(TParent));
        /// <summary>Gets the parent object.</summary>
        /// <remarks>The value of parent object can only be retrieved inside the constructor callback. When called
        /// outside the constructor callback, the default value of <typeparamref name="TParent"/> will be returned.</remarks>
        public TParent Parent
        {
            get { return s_parent.Value; }
            private set { s_parent.Value = value; }
        }

        /// <summary>Gets the property value of the specified parent object.</summary>
        /// <param name="parent">The parent object.</param>
        /// <returns>The property value.</returns>
        public TProperty GetProperty(TParent parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            return Getter(parent);
        }

        internal TProperty Construct(TParent target)
        {
            Parent = target;
            var result = Constructor();
            Setter(target, result);
            Parent = default(TParent);
            return result;
        }
    }
}
