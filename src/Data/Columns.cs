using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>Manipulates set of columns.</summary>
    /// <remarks>
    /// <para><see cref="Column"/> class implements <see cref="IColumns"/>, so a <see cref="Column"/> instance can represent both
    /// the column itself and a singleton set of columns. This can improve performance by avoiding unnecessary object creation.</para>
    /// <para><see cref="IColumns"/> can be sealed as immutable, any change to <see cref="IColumns"/> may or may not
    /// create a new <see cref="IColumns"/> instance. Consumer of <see cref="IColumns"/> should always assume it's immutable.</para>
    /// </remarks>
    public static class Columns
    {
        private class EmptyColumns : IColumns
        {
            public static EmptyColumns Singleton = new EmptyColumns();
            private EmptyColumns()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public bool Contains(Column value)
            {
                value.VerifyNotNull(nameof(value));
                return false;
            }

            public IColumns Add(Column value)
            {
                value.VerifyNotNull(nameof(value));
                return value;
            }

            public IColumns Remove(Column value)
            {
                value.VerifyNotNull(nameof(value));
                return this;
            }

            public IColumns Clear()
            {
                return this;
            }

            public IColumns Seal()
            {
                return this;
            }

            public IEnumerator<Column> GetEnumerator()
            {
                return EmptyEnumerator<Column>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<Column>.Singleton;
            }

            public override string ToString()
            {
                return "[]";
            }
        }

        private class HashSetColumns : IColumns
        {
            private bool _isSealed;
            private HashSet<Column> _hashSet = new HashSet<Column>();

            public HashSetColumns(Column value1, Column value2)
            {
                Debug.Assert(value1 != null && value2 != null && value1 != value2);
                Add(value1);
                Add(value2);
            }

            private HashSetColumns()
            {
            }

            public bool IsSealed
            {
                get { return _isSealed; }
            }

            public int Count
            {
                get { return _hashSet.Count; }
            }

            public IColumns Seal()
            {
                _isSealed = true;
                return this;
            }

            public IColumns Add(Column value)
            {
                value.VerifyNotNull(nameof(value));

                if (Contains(value))
                    return this;

                if (!IsSealed)
                {
                    _hashSet.Add(value);
                    return this;
                }

                if (Count == 0)
                    return value;
                else
                {
                    var result = new HashSetColumns();
                    foreach (var column in this)
                        result.Add(column);
                    result.Add(value);
                    return result;
                }
            }

            public IColumns Remove(Column value)
            {
                value.VerifyNotNull(nameof(value));

                if (!Contains(value))
                    return this;

                if (Count == 1)
                    return Empty;

                if (Count == 2)
                    return _hashSet.Single(x => x != value);

                if (!IsSealed)
                {
                    _hashSet.Remove(value);
                    return this;
                }

                var result = new HashSetColumns();
                foreach (var element in this)
                {
                    if (element != value)
                        result.Add(element);
                }
                return result;
            }

            public IColumns Clear()
            {
                if (IsSealed)
                    return Empty;
                else
                {
                    _hashSet.Clear();
                    return this;
                }
            }

            public bool Contains(Column value)
            {
                value.VerifyNotNull(nameof(value));
                return _hashSet.Contains(value);
            }

            private static IEqualityComparer<HashSet<Column>> s_setComparer = HashSet<Column>.CreateSetComparer();
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;
                return !(obj is HashSetColumns columns) ? false : s_setComparer.Equals(_hashSet, columns._hashSet);
            }

            public override int GetHashCode()
            {
                return s_setComparer.GetHashCode(_hashSet);
            }

            public IEnumerator<Column> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }

            public override string ToString()
            {
                return string.Format("[{0}]", string.Join(", ", _hashSet.Select(x => string.Format("'{0}'", x)).ToArray()));
            }
        }

        /// <summary>
        /// Gets an empty columns set.
        /// </summary>
        public static IColumns Empty
        {
            get { return EmptyColumns.Singleton; }
        }

        internal static IColumns New(Column value1, Column value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new HashSetColumns(value1, value2);
        }

        /// <summary>
        /// Creates a new set of columns.
        /// </summary>
        /// <param name="values">The value of columns.</param>
        /// <returns>The column set.</returns>
        public static IColumns New(params Column[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IColumns result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }

        internal static string Serialize(this IColumns columns)
        {
            return columns == null || columns.Count == 0 ? string.Empty : string.Join(",", columns.Select(x => x.Name));
        }

        internal static IColumns Deserialize(Model model, string input)
        {
            if (string.IsNullOrEmpty(input))
                return Columns.Empty;

            var columnNames = input.Split(',');
            if (columnNames == null || columnNames.Length == 0)
                return Columns.Empty;

            var result = new Column[columnNames.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = model.DeserializeColumn(columnNames[i]);

            return Columns.New(result);
        }

        /// <summary>Removes specified columns from the current columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to be removed.</param>
        /// <returns>A new set of columns if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumns Except(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var column in source)
            {
                if (other.Contains(column))
                    source = source.Remove(column);
            }
            return source;
        }

        /// <summary>Removes the columns to ensure the set contains only columns both exist in the current columns and the specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns>A new set of columns if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumns Intersect(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var column in source)
            {
                if (!other.Contains(column))
                    source = source.Remove(column);
            }
            return source;
        }

        private static bool ContainsAll(this IColumns source, IColumns other)
        {
            foreach (var column in other)
            {
                if (!source.Contains(column))
                    return false;
            }
            return true;
        }

        /// <summary>Determines whether the current columns is a proper (strict) subset of the specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSubsetOf(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count < other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current columns is a proper (strict) superset of the specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSupersetOf(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count > other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current columns is a subset of a specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSubsetOf(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count <= other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current columns is a superset of a specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSupersetOf(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count >= other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current columns overlaps with the specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set overlaps with the specified columns; otherwise, <see langword="false" />.</returns>
        public static bool Overlaps(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var column in source)
            {
                if (other.Contains(column))
                    return true;
            }
            return false;
        }

        /// <summary>Determines whether the current columns and the specified columns contain the same elements.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns><see langword="true"/> if the current set and the specified columns contain the same elements; otherwise, <see langword="false" />.</returns>
        public static bool SetEquals(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count == other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Ensures set contain only elements that are present either in the current columns or in the specified columns, but not both.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The columns to compare to the current columns.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumns SymmetricExcept(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            IColumns removedColumnSet = Columns.Empty;
            foreach (var column in source)
            {
                if (other.Contains(column))
                {
                    removedColumnSet = removedColumnSet.Add(column);
                    source = source.Remove(column);
                }
            }

            foreach (var column in other)
            {
                if (removedColumnSet.Contains(column))
                    source = source.Add(column);
            }

            return source;
        }

        /// <summary>Ensures set contain all elements that are present in either the current columns or in the specified columns.</summary>
        /// <param name="source">The current columns.</param>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IColumns Union(this IColumns source, IColumns other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var column in other)
                source = source.Add(column);
            return source;
        }
    }
}
