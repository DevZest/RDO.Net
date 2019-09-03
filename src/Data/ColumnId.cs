using System;

namespace DevZest.Data
{
    /// <summary>A structure contains owner type and name to identify a <see cref="Column"/>.</summary>
    public struct ColumnId : IEquatable<ColumnId>
    {
        internal ColumnId(Type declaringType, string name)
        {
            DeclaringType = declaringType;
            Name = name;
        }

        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = DeclaringType != null ? DeclaringType.GetHashCode() : 0;
                if (Name != null)
                    hash += hash * 31 + Name.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is ColumnId) ? Equals((ColumnId)obj) : false;
        }

        /// <summary>Determines whether a specified <see cref="ColumnId"/> struct equals current <see cref="ColumnId"/> struct.</summary>
        /// <param name="other">The <see cref="ColumnId"/> struct for the equality comparison.</param>
        /// <returns><see langword="true"/> if equals, otherwise <see langword="false"/>.</returns>
        public bool Equals(ColumnId other)
        {
            return DeclaringType == other.DeclaringType && Name == other.Name;
        }

        /// <summary>Determines whether two <see cref="ColumnId"/> structs are equal.</summary>
        /// <param name="x">The <see cref="ColumnId"/> struct.</param>
        /// <param name="y">Another <see cref="ColumnId"/> struct.</param>
        /// <returns><see langword="true"/> if two <see cref="ColumnId"/> structs are equal, otherwise <see langword="false"/></returns>
        public static bool operator ==(ColumnId x, ColumnId y)
        {
            return x.Equals(y);
        }

        /// <summary>Determines whether two <see cref="ColumnId"/> structs are not equal.</summary>
        /// <param name="x">The <see cref="ColumnId"/> struct.</param>
        /// <param name="y">Another <see cref="ColumnId"/> struct.</param>
        /// <returns><see langword="true"/> if two <see cref="ColumnId"/> structs are not equal, otherwise <see langword="false"/></returns>
        public static bool operator !=(ColumnId x, ColumnId y)
        {
            return !(x == y);
        }
    }
}
