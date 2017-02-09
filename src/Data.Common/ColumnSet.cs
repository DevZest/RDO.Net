using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    public static class ColumnSet
    {
        private class EmptyColumnSet : IColumnSet
        {
            public static EmptyColumnSet Singleton = new EmptyColumnSet();
            private EmptyColumnSet()
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
                Check.NotNull(value, nameof(value));
                return false;
            }

            public IColumnSet Add(Column value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IColumnSet Remove(Column value)
            {
                Check.NotNull(value, nameof(value));
                return this;
            }

            public IColumnSet Clear()
            {
                return this;
            }

            public IColumnSet Seal()
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
        }

        private class HashSetColumnSet : IColumnSet
        {
            private bool _isSealed;
            private HashSet<Column> _hashSet = new HashSet<Column>();

            public HashSetColumnSet(Column value1, Column value2)
            {
                Debug.Assert(value1 != null && value2 != null && value1 != value2);
                Add(value1);
                Add(value2);
            }

            private HashSetColumnSet()
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

            public IColumnSet Seal()
            {
                _isSealed = true;
                return this;
            }

            public IColumnSet Add(Column value)
            {
                Check.NotNull(value, nameof(value));

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
                    var result = new HashSetColumnSet();
                    foreach (var column in this)
                        result.Add(column);
                    result.Add(value);
                    return result;
                }
            }

            public IColumnSet Remove(Column value)
            {
                Check.NotNull(value, nameof(value));

                if (!Contains(value))
                    return this;

                if (!IsSealed)
                {
                    _hashSet.Remove(value);
                    return this;
                }

                if (Count == 1)
                    return Empty;

                var result = new HashSetColumnSet();
                foreach (var element in this)
                {
                    if (element != value)
                        result.Add(element);
                }
                return result;
            }

            public IColumnSet Clear()
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
                Check.NotNull(value, nameof(value));
                return _hashSet.Contains(value);
            }

            public IEnumerator<Column> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }
        }

        public static IColumnSet Empty
        {
            get { return EmptyColumnSet.Singleton; }
        }

        internal static IColumnSet New(Column value1, Column value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new HashSetColumnSet(value1, value2);
        }

        public static IColumnSet New(params Column[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IColumnSet result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }

        internal static string Serialize(this IColumnSet columns)
        {
            return columns == null || columns.Count == 0 ? string.Empty : string.Join(",", columns.Select(x => x.Name));
        }

        internal static IColumnSet Deserialize(Model model, string input)
        {
            if (string.IsNullOrEmpty(input))
                return ColumnSet.Empty;

            var columnNames = input.Split(',');
            if (columnNames == null || columnNames.Length == 0)
                return ColumnSet.Empty;

            var result = new Column[columnNames.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = model.DeserializeColumn(columnNames[i]);

            return ColumnSet.New(result);
        }

        /// <summary>Removes the columns in the specified collection from the current set.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumnSet Except(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in source)
            {
                if (other.Contains(column))
                    source = source.Remove(column);
            }
            return source;
        }

        /// <summary>Removes the columns to ensure the set contains only columns both exist in this set and the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IColumnSet Intersect(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in source)
            {
                if (!other.Contains(column))
                    source = source.Remove(column);
            }
            return source;
        }

        private static bool ContainsAll(this IColumnSet source, IColumnSet other)
        {
            foreach (var column in other)
            {
                if (!source.Contains(column))
                    return false;
            }
            return true;
        }

        /// <summary>Determines whether the current set is a proper (strict) subset of the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSubsetOf(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count < other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSupersetOf(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count > other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSubsetOf(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count <= other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSupersetOf(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count >= other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set overlaps with the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool Overlaps(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in source)
            {
                if (other.Contains(column))
                    return true;
            }
            return false;
        }

        /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set and the specified collection contain the same elements; otherwise, <see langword="false" />.</returns>
        public static bool SetEquals(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count == other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Ensures set contain only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumnSet SymmetricExcept(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            IColumnSet removedColumnSet = ColumnSet.Empty;
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

        /// <summary>Ensures set contain all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IColumnSet Union(this IColumnSet source, IColumnSet other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in other)
                source = source.Add(column);
            return source;
        }
    }
}
