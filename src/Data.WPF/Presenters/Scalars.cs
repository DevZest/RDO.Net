using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Presenters
{
    public static class Scalars
    {
        private class EmptyScalarSet : IScalars
        {
            public static EmptyScalarSet Singleton = new EmptyScalarSet();
            private EmptyScalarSet()
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

            public bool Contains(Scalar value)
            {
                Check.NotNull(value, nameof(value));
                return false;
            }

            public IScalars Add(Scalar value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IScalars Remove(Scalar value)
            {
                Check.NotNull(value, nameof(value));
                return this;
            }

            public IScalars Clear()
            {
                return this;
            }

            public IScalars Seal()
            {
                return this;
            }

            public IEnumerator<Scalar> GetEnumerator()
            {
                return EmptyEnumerator<Scalar>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<Scalar>.Singleton;
            }
        }

        private class HashSetScalarSet : IScalars
        {
            private bool _isSealed;
            private HashSet<Scalar> _hashSet = new HashSet<Scalar>();

            public HashSetScalarSet(Scalar value1, Scalar value2)
            {
                Debug.Assert(value1 != null && value2 != null && value1 != value2);
                Add(value1);
                Add(value2);
            }

            private HashSetScalarSet()
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

            public IScalars Seal()
            {
                _isSealed = true;
                return this;
            }

            public IScalars Add(Scalar value)
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
                    var result = new HashSetScalarSet();
                    foreach (var column in this)
                        result.Add(column);
                    result.Add(value);
                    return result;
                }
            }

            public IScalars Remove(Scalar value)
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

                var result = new HashSetScalarSet();
                foreach (var element in this)
                {
                    if (element != value)
                        result.Add(element);
                }
                return result;
            }

            public IScalars Clear()
            {
                if (IsSealed)
                    return Empty;
                else
                {
                    _hashSet.Clear();
                    return this;
                }
            }

            public bool Contains(Scalar value)
            {
                Check.NotNull(value, nameof(value));
                return _hashSet.Contains(value);
            }

            public IEnumerator<Scalar> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }
        }

        public static IScalars Empty
        {
            get { return EmptyScalarSet.Singleton; }
        }

        internal static IScalars New(Scalar value1, Scalar value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new HashSetScalarSet(value1, value2);
        }

        public static IScalars New(params Scalar[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IScalars result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }

        /// <summary>Removes the columns in the specified collection from the current set.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IScalars Except(this IScalars source, IScalars other)
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
        public static IScalars Intersect(this IScalars source, IScalars other)
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

        private static bool ContainsAll(this IScalars source, IScalars other)
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
        public static bool IsProperSubsetOf(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count < other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSupersetOf(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count > other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSubsetOf(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count <= other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSupersetOf(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count >= other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set overlaps with the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool Overlaps(this IScalars source, IScalars other)
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
        public static bool SetEquals(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            return source.Count == other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Ensures set contain only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IScalars SymmetricExcept(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            IScalars removedScalarSet = Scalars.Empty;
            foreach (var column in source)
            {
                if (other.Contains(column))
                {
                    removedScalarSet = removedScalarSet.Add(column);
                    source = source.Remove(column);
                }
            }

            foreach (var column in other)
            {
                if (removedScalarSet.Contains(column))
                    source = source.Add(column);
            }

            return source;
        }

        /// <summary>Ensures set contain all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IScalars Union(this IScalars source, IScalars other)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(other, nameof(other));

            foreach (var column in other)
                source = source.Add(column);
            return source;
        }
    }
}
