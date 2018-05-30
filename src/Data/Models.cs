using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public static class Models
    {
        private class EmptyModelSet : IModels
        {
            public static EmptyModelSet Singleton = new EmptyModelSet();

            private EmptyModelSet()
            {
            }

            public bool Contains(Model model)
            {
                return false;
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IEnumerator<Model> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }

            public IModels Seal()
            {
                return this;
            }

            public IModels Add(Model value)
            {
                value.VerifyNotNull(nameof(value));
                return value;
            }

            public IModels Remove(Model value)
            {
                value.VerifyNotNull(nameof(value));
                return this;
            }

            public IModels Clear()
            {
                return this;
            }
        }

        public static IModels Empty
        {
            get { return EmptyModelSet.Singleton; }
        }


        private class HashSetModelSet : IModels
        {
            private bool _isSealed;
            private HashSet<Model> _hashSet = new HashSet<Model>();

            public HashSetModelSet(Model value1, Model value2)
            {
                Debug.Assert(value1 != null && value2 != null && value1 != value2);
                Add(value1);
                Add(value2);
            }

            private HashSetModelSet()
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

            public IModels Seal()
            {
                _isSealed = true;
                return this;
            }

            public IModels Add(Model value)
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
                    var result = new HashSetModelSet();
                    foreach (var model in this)
                        result.Add(model);
                    result.Add(value);
                    return result;
                }
            }

            public IModels Remove(Model value)
            {
                value.VerifyNotNull(nameof(value));

                if (!Contains(value))
                    return this;

                if (!IsSealed)
                {
                    _hashSet.Remove(value);
                    return this;
                }

                if (Count == 1)
                    return Empty;

                var result = new HashSetModelSet();
                foreach (var element in this)
                {
                    if (element != value)
                        result.Add(element);
                }
                return result;
            }

            public IModels Clear()
            {
                if (IsSealed)
                    return Empty;
                else
                {
                    _hashSet.Clear();
                    return this;
                }
            }

            public bool Contains(Model value)
            {
                value.VerifyNotNull(nameof(value));
                return _hashSet.Contains(value);
            }

            public IEnumerator<Model> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }
        }

        internal static IModels New(Model value1, Model value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new HashSetModelSet(value1, value2);
        }

        public static IModels New(params Model[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IModels result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }

        /// <summary>Removes the models in the specified collection from the current set.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IModels Except(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var item in source)
            {
                if (other.Contains(item))
                    source = source.Remove(item);
            }
            return source;
        }

        /// <summary>Removes the models to ensure the set contains only models both exist in this set and the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IModels Intersect(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var item in source)
            {
                if (!other.Contains(item))
                    source = source.Remove(item);
            }
            return source;
        }

        private static bool ContainsAll(this IModels source, IModels other)
        {
            foreach (var item in other)
            {
                if (!source.Contains(item))
                    return false;
            }
            return true;
        }

        public static bool ContainsAny(this IModels source, IModels other)
        {
            foreach (var item in other)
            {
                if (source.Contains(item))
                    return true;
            }
            return false;
        }

        /// <summary>Determines whether the current set is a proper (strict) subset of the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSubsetOf(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count < other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsProperSupersetOf(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count > other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSubsetOf(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count <= other.Count ? other.ContainsAll(source) : false;
        }

        /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool IsSupersetOf(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count >= other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set overlaps with the specified collection; otherwise, <see langword="false" />.</returns>
        public static bool Overlaps(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var item in source)
            {
                if (other.Contains(item))
                    return true;
            }
            return false;
        }

        /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see cref="true"/> if the current set and the specified collection contain the same elements; otherwise, <see langword="false" />.</returns>
        public static bool SetEquals(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            return source.Count == other.Count ? source.ContainsAll(other) : false;
        }

        /// <summary>Ensures set contain only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set if there is any modification to current sealed set; otherwise, the current set.</returns>
        public static IModels SymmetricExcept(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            IModels removedModelSet = Models.Empty;
            foreach (var item in source)
            {
                if (other.Contains(item))
                {
                    removedModelSet = removedModelSet.Add(item);
                    source = source.Remove(item);
                }
            }

            foreach (var item in other)
            {
                if (removedModelSet.Contains(item))
                    source = source.Add(item);
            }

            return source;
        }

        /// <summary>Ensures set contain all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="source">The current set.</param>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new set if there is any modification to current set and current set sealed; otherwise, the current set.</returns>
        public static IModels Union(this IModels source, IModels other)
        {
            source.VerifyNotNull(nameof(source));
            other.VerifyNotNull(nameof(other));

            foreach (var item in other)
                source = source.Add(item);
            return source;
        }

        private sealed class SourceModelResolver : DbFromClauseVisitor
        {
            public SourceModelResolver(IModels sourceModelSet)
            {
                SourceModelSet = sourceModelSet;
            }

            public IModels SourceModelSet { get; private set; }

            public override void Visit(DbUnionStatement union)
            {
                union.Query1.Accept(this);
                union.Query2.Accept(this);
            }

            public override void Visit(DbJoinClause join)
            {
                join.Left.Accept(this);
                join.Right.Accept(this);
            }

            public override void Visit(DbSelectStatement select)
            {
                SourceModelSet = SourceModelSet.Add(select.Model);
            }

            public override void Visit(DbTableClause table)
            {
                SourceModelSet = SourceModelSet.Add(table.Model);
            }
        }

        public static IModels Add(this IModels modelSet, DbFromClause dbFromClause)
        {
            var resolver = new SourceModelResolver(modelSet);
            dbFromClause.Accept(resolver);
            return resolver.SourceModelSet.Seal();
        }
    }
}
