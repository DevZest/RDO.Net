using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class JsonFilter
    {
        public static JsonFilter NoExtraColumns { get { return NoExtraColumnsJsonFilter.Singleton; } }
        public static JsonFilter NoChildDataSet { get { return NoChildDataSetJsonFilter.Singleton; } }
        public static JsonFilter PrimaryKeyOnly { get { return PrimaryKeyOnlyJsonFilter.Singleton; } }
        public static JsonFilter Explicit(params ModelMember[] members)
        {
            return new ExplicitMembersJsonFilter(Verify(members, nameof(members)));
        }
        public static JsonFilter Explicit(params ColumnGroup[] columnCombinations)
        {
            return new ExplicitExtraColumnsJsonFilter(Verify(columnCombinations, nameof(columnCombinations)));
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

        protected internal abstract bool ShouldSerialize(ColumnGroup columnGroup);

        private sealed class NoExtraColumnsJsonFilter : JsonFilter
        {
            public static readonly NoExtraColumnsJsonFilter Singleton = new NoExtraColumnsJsonFilter();

            private NoExtraColumnsJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ColumnGroup ext)
            {
                return false;
            }

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return true;
            }
        }

        private sealed class NoChildDataSetJsonFilter : JsonFilter
        {
            public static readonly NoChildDataSetJsonFilter Singleton = new NoChildDataSetJsonFilter();

            private NoChildDataSetJsonFilter()
            {
            }

            protected internal override bool ShouldSerialize(ColumnGroup ext)
            {
                return true;
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

            protected internal override bool ShouldSerialize(ColumnGroup ext)
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

            protected internal override bool ShouldSerialize(ColumnGroup ext)
            {
                return true;
            }
        }

        private sealed class ExplicitExtraColumnsJsonFilter : JsonFilter
        {
            public ExplicitExtraColumnsJsonFilter(HashSet<ColumnGroup> columnCombinations)
            {
                Debug.Assert(columnCombinations != null);
                _columnCombinations = columnCombinations;
            }

            private readonly HashSet<ColumnGroup> _columnCombinations;

            protected internal override bool ShouldSerialize(ModelMember member)
            {
                return true;
            }

            protected internal override bool ShouldSerialize(ColumnGroup ext)
            {
                return _columnCombinations.Contains(ext);
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

            protected internal override bool ShouldSerialize(ColumnGroup ext)
            {
                for (int i = 0; i < _filters.Length; i++)
                {
                    if (!_filters[i].ShouldSerialize(ext))
                        return false;
                }
                return true;
            }
        }
    }
}
