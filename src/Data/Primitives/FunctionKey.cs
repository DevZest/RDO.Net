using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Identifies a function implementation.
    /// </summary>
    public sealed class FunctionKey
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FunctionKey"/>.
        /// </summary>
        /// <param name="declaringType">The type which declares this function key.</param>
        /// <param name="name">The name of this function key.</param>
        public FunctionKey(Type declaringType, string name)
        {
            DeclaringType = declaringType;
            Name = name;
        }

        /// <summary>
        /// Gets the type which declares this function key.
        /// </summary>
        public Type DeclaringType { get; private set; }

        /// <summary>
        /// Gets the name of this function key.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}.{1}", DeclaringType, Name);
        }
    }
}
