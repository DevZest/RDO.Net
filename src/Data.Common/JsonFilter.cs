using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class JsonFilter
    {
        public static JsonFilter NoExtender { get { return NoExtenderJsonFilter.Singleton; } }
        public static JsonFilter PrimaryKeyOnly { get { return PrimaryKeyOnlyJsonFilter.Singleton; } }
        public static JsonFilter Explicit(params ModelMember[] members)
        {
            return new ExplicitMembersJsonFilter(Verify(members, nameof(members)));
        }
        public static JsonFilter Explicit(params ModelExtender[] extenders)
        {
            return new ExplicitExtendersJsonFilter(Verify(extenders, nameof(extenders)));
        }

        public static JsonFilter Join(params JsonFilter[] filters)
        {
            Check.NotNull(filters, nameof(filters));
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
            Check.NotNull(items, paramName);
            var result = new HashSet<T>();
            for (int i = 0; i < paramName.Length; i++)
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

        protected internal abstract bool ShouldSerialize(ModelExtender extender);

        private sealed class NoExtenderJsonFilter : JsonFilter
        {
            public static readonly NoExtenderJsonFilter Singleton = new NoExtenderJsonFilter();

            private NoExtenderJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ModelExtender extender)
            {
                return false;
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return true;
            }
        }

        private sealed class PrimaryKeyOnlyJsonFilter : JsonFilter
        {
            public static readonly PrimaryKeyOnlyJsonFilter Singleton = new PrimaryKeyOnlyJsonFilter();

            private PrimaryKeyOnlyJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ModelExtender extender)
            {
                return false;
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                var column = member as Column;
                return column != null && column.IsPrimaryKey;
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

            protected internal override bool ShouldSerialize(ModelExtender extender)
            {
                return true;
            }
        }

        private sealed class ExplicitExtendersJsonFilter : JsonFilter
        {
            public ExplicitExtendersJsonFilter(HashSet<ModelExtender> extenders)
            {
                Debug.Assert(extenders != null);
                _extenders = extenders;
            }

            private readonly HashSet<ModelExtender> _extenders;

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return true;
            }

            protected internal override bool ShouldSerialize(ModelExtender extender)
            {
                return _extenders.Contains(extender);
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

            protected internal override bool ShouldSerialize(ModelExtender extender)
            {
                for (int i = 0; i < _filters.Length; i++)
                {
                    if (!_filters[i].ShouldSerialize(extender))
                        return false;
                }
                return true;
            }
        }
    }
}
