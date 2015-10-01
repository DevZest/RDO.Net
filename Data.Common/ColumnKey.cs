using System;

namespace DevZest.Data
{
    /// <summary>A structure which can uniquely identify a <see cref="Column"/> in a <see cref="Model"/>.</summary>
    public struct ColumnKey : IEquatable<ColumnKey>
    {
        internal ColumnKey(Type originalOwnerType, string originalName)
        {
            OriginalOwnerType = originalOwnerType;
            OriginalName = originalName;
        }

        /// <inheritdoc cref="Column.OriginalOwnerType" select="summary"/>
        public readonly Type OriginalOwnerType;

        /// <inheritdoc cref="Column.OriginalName" select="summary"/>
        public readonly string OriginalName;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = OriginalOwnerType != null ? OriginalOwnerType.GetHashCode() : 0;
                if (OriginalName != null)
                    hash += hash * 31 + OriginalName.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is ColumnKey) ? Equals((ColumnKey)obj) : false;
        }

        /// <summary>Determines whether a specified <see cref="ColumnKey"/> struct equals current <see cref="ColumnKey"/> struct.</summary>
        /// <param name="other">The <see cref="ColumnKey"/> struct for the equality comparison.</param>
        /// <returns><see langword="true"/> if equals, otherwise <see langword="false"/>.</returns>
        public bool Equals(ColumnKey other)
        {
            return OriginalOwnerType == other.OriginalOwnerType && OriginalName == other.OriginalName;
        }

        /// <summary>Determines whether two <see cref="ColumnKey"/> structs are equal.</summary>
        /// <param name="x">The <see cref="ColumnKey"/> struct.</param>
        /// <param name="y">Another <see cref="ColumnKey"/> struct.</param>
        /// <returns><see langword="true"/> if two <see cref="ColumnKey"/> structs are equal, otherwise <see langword="false"/></returns>
        public static bool operator ==(ColumnKey x, ColumnKey y)
        {
            return x.Equals(y);
        }

        /// <summary>Determines whether two <see cref="ColumnKey"/> structs are not equal.</summary>
        /// <param name="x">The <see cref="ColumnKey"/> struct.</param>
        /// <param name="y">Another <see cref="ColumnKey"/> struct.</param>
        /// <returns><see langword="true"/> if two <see cref="ColumnKey"/> structs are not equal, otherwise <see langword="false"/></returns>
        public static bool operator !=(ColumnKey x, ColumnKey y)
        {
            return !(x == y);
        }
    }
}
