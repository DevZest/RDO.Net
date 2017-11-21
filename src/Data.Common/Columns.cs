﻿using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    public static class Columns
    {
        private class EmptyColumnSet : IColumns
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

            public IColumns Add(Column value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IColumns Remove(Column value)
            {
                Check.NotNull(value, nameof(value));
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

        private class HashSetColumnSet : IColumns
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

            public IColumns Seal()
            {
                _isSealed = true;
                return this;
            }

            public IColumns Add(Column value)
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

            public IColumns Remove(Column value)
            {
                Check.NotNull(value, nameof(value));

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

                var result = new HashSetColumnSet();
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

            public override string ToString()
            {
                return string.Format("[{0}]", string.Join(", ", _hashSet.Select(x => string.Format("'{0}'", x)).ToArray()));
            }
        }

        public static IColumns Empty
        {
            get { return EmptyColumnSet.Singleton; }
        }

        internal static IColumns New(Column value1, Column value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new HashSetColumnSet(value1, value2);
        }

        public static IColumns New(params Column[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IColumns result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
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

        /// <summary>Removes the columns in the specified collection from the current set.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumns Except(this IColumns source, IColumns other)
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
        public static IColumns Intersect(this IColumns source, IColumns other)
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

        private static bool ContainsAll(this IColumns source, IColumns other)
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
        public static bool IsProperSubsetOf(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count < other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSupersetOf(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count > other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSubsetOf(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count <= other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSupersetOf(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count >= other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set overlaps with the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool Overlaps(this IColumns source, IColumns other)
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
        public static bool SetEquals(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count == other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Ensures set contain only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IColumns SymmetricExcept(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

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

        /// <summary>Ensures set contain all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IColumns Union(this IColumns source, IColumns other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in other)
                source = source.Add(column);
            return source;
        }
    }
}
