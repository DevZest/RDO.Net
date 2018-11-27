using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class JsonFilter
    {
        public static JsonFilter NoProjection { get { return NoProjectionJsonFilter.Singleton; } }
        public static JsonFilter NoChildDataSet { get { return NoChildDataSetJsonFilter.Singleton; } }
        public static JsonFilter PrimaryKeyOnly { get { return PrimaryKeyOnlyJsonFilter.Singleton; } }

        public static JsonFilter Explicit(params ModelMember[] members)
        {
            return new ExplicitMembersJsonFilter(Verify(members, nameof(members)));
        }

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
