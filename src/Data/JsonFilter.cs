using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Filters DataSet JSON serialization.
    /// </summary>
    public abstract class JsonFilter
    {
        /// <summary>
        /// Gets the filter which excludes all projections.
        /// </summary>
        public static JsonFilter NoProjection { get { return NoProjectionJsonFilter.Singleton; } }

        /// <summary>
        /// Gets the filter which exclude all child DataSets.
        /// </summary>
        public static JsonFilter NoChildDataSet { get { return NoChildDataSetJsonFilter.Singleton; } }

        /// <summary>
        /// Gets the filter which contains primary key values only.
        /// </summary>
        public static JsonFilter PrimaryKeyOnly { get { return PrimaryKeyOnlyJsonFilter.Singleton; } }

        /// <summary>
        /// Creates a filter explicitly.
        /// </summary>
        /// <param name="members">The model members included for JSON serialization.</param>
        /// <returns>The created filter.</returns>
        public static JsonFilter Explicit(params ModelMember[] members)
        {
            return new ExplicitMembersJsonFilter(Verify(members, nameof(members)));
        }

        /// <summary>
        /// Joins multiple filters into a single filter.
        /// </summary>
        /// <param name="filters">The multiple filters.</param>
        /// <returns>The result single filter.</returns>
        public static JsonFilter Join(params JsonFilter[] filters)
        {
            filters.VerifyNotNull(nameof(filters));
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i] == null)
                    throw new ArgumentNullException(string.Format("{0}.[{1}]", nameof(filters), i));
            }
            return new JoinedJsonFilter(filters);
        }

        private static HashSet<T> Verify<T>(T[] items, string paramName)
            where T : class
        {
            items.VerifyNotNull(paramName);
            var result = new HashSet<T>();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                    throw new ArgumentNullException(string.Format("{0}.[{1}]", paramName, i));
                if (!result.Contains(item))
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Determines whether specified model member should be serialized.
        /// </summary>
        /// <param name="member">The specified model member.</param>
        /// <returns><see langword="true"/> if specified model member should be serialized, otherwise <see langword="false"/>.</returns>
        protected internal abstract bool ShouldSerialize(ModelMember member);

        private sealed class NoProjectionJsonFilter : JsonFilter
        {
            public static readonly NoProjectionJsonFilter Singleton = new NoProjectionJsonFilter();

            private NoProjectionJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return !(member is Projection);
            }
        }

        private sealed class NoChildDataSetJsonFilter : JsonFilter
        {
            public static readonly NoChildDataSetJsonFilter Singleton = new NoChildDataSetJsonFilter();

            private NoChildDataSetJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                var model = member as Model;
                return model == null;
            }
        }

        private sealed class PrimaryKeyOnlyJsonFilter : JsonFilter
        {
            public static readonly PrimaryKeyOnlyJsonFilter Singleton = new PrimaryKeyOnlyJsonFilter();

            private PrimaryKeyOnlyJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return member is Column column && column.IsPrimaryKey;
            }
        }

        private sealed class ExplicitMembersJsonFilter : JsonFilter
        {
            public ExplicitMembersJsonFilter(HashSet<ModelMember> members)
            {
                Debug.Assert(members != null);
                _members = members;
            }

            private readonly HashSet<ModelMember> _members;

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return _members.Contains(member);
            }
        }

        private sealed class JoinedJsonFilter : JsonFilter
        {
            public JoinedJsonFilter(JsonFilter[] filters)
            {
                Debug.Assert(filters != null);
                _filters = filters;
            }

            private readonly JsonFilter[] _filters;

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                for (int i = 0; i < _filters.Length; i++)
                {
                    if (!_filters[i].ShouldSerialize(member))
                        return false;
                }
                return true;
            }
        }
    }
}
