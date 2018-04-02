using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbDelete<T> : Executable<int>
        where T : Model, new()
    {
        internal static DbDelete<T> Create(DbTable<T> from, Func<T, _Boolean> where)
        {
            return new DbDeleteWhere(from, where);
        }

        internal static DbDelete<T> Create<TLookup>(DbTable<T> from, DbSet<TLookup> lookup, IReadOnlyList<ColumnMapping> columnMappings)
            where TLookup : Model, new()
        {
            return new DbDeleteFromDbSet<TLookup>(from, lookup, columnMappings);
        }

        internal static DbDelete<T> Create<TLookup>(DbTable<T> from, DataSet<TLookup> lookup, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings)
            where TLookup : Model, new()
        {
            return new DbDeleteFromDataRow<TLookup>(from, lookup, rowIndex, columnMappings);
        }

        internal static DbDelete<T> Create<TLookup>(DbTable<T> from, DataSet<TLookup> lookup, PrimaryKey keyMappingTarget)
            where TLookup : Model, new()
        {
            return new DbDeleteFromDataSet<TLookup>(from, lookup, keyMappingTarget);
        }

        protected DbDelete(DbTable<T> from)
        {
            Debug.Assert(from != null);
            _from = from;
        }

        private readonly DbTable<T> _from;
        protected DbTable<T> From
        {
            get { return _from; }
        }

        protected DbSession DbSession
        {
            get { return From.DbSession; }
        }

        private sealed class DbDeleteWhere : DbDelete<T>
        {
            public DbDeleteWhere(DbTable<T> from, Func<T, _Boolean> where)
                : base(from)
            {
                _where = where;
            }

            private readonly Func<T, _Boolean> _where;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteStatement(_where);
            }

            protected override int PerformExecute()
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, DbSession.Delete(statement));
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, await DbSession.DeleteAsync(statement, ct));
            }
        }
            

        private sealed class DbDeleteFromDbSet<TSource> : DbDelete<T>
            where TSource : Model, new()
        {
            public DbDeleteFromDbSet(DbTable<T> from, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings)
                : base(from)
            {
                _source = source;
                _columnMappings = columnMappings;
            }

            private readonly DbSet<TSource> _source;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteStatement(_source, _columnMappings);
            }

            protected override int PerformExecute()
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, DbSession.Delete(statement));
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin(null, await DbSession.DeleteAsync(statement, ct));
            }
        }

        private sealed class DbDeleteFromDataRow<TSource> : DbDelete<T>
            where TSource : Model, new()
        {
            public DbDeleteFromDataRow(DbTable<T> from, DataSet<TSource> source, int rowIndex, IReadOnlyList<ColumnMapping> columnMappings)
                : base(from)
            {
                _source = source;
                _rowIndex = rowIndex;
                _columnMappings = columnMappings;
            }

            private readonly DataSet<TSource> _source;
            private readonly int _rowIndex;
            private readonly IReadOnlyList<ColumnMapping> _columnMappings;

            private DbSelectStatement BuildDeleteStatement()
            {
                return From.BuildDeleteScalarStatement(_source, _rowIndex, _columnMappings);
            }

            protected override int PerformExecute()
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin<TSource>(null, DbSession.Delete(statement) > 0) ? 1 : 0;
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                var statement = BuildDeleteStatement();
                return From.UpdateOrigin<TSource>(null, await DbSession.DeleteAsync(statement, ct) > 0) ? 1 : 0;
            }
        }

        private sealed class DbDeleteFromDataSet<TLookup> : DbDelete<T>
            where TLookup : Model, new()
        {
            public DbDeleteFromDataSet(DbTable<T> from, DataSet<TLookup> lookup, PrimaryKey keyMappingTarget)
                : base(from)
            {
                Debug.Assert(lookup.Count != 1);
                _lookup = lookup;
                _keyMappingTarget = keyMappingTarget;
            }

            private readonly DataSet<TLookup> _lookup;
            private readonly PrimaryKey _keyMappingTarget;

            protected override int PerformExecute()
            {
                if (_lookup.Count == 0)
                    return 0;
                return DbSession.Delete(_lookup, From, _keyMappingTarget);
            }

            protected override async Task<int> PerformExecuteAsync(CancellationToken ct)
            {
                if (_lookup.Count == 0)
                    return 0;
                return await DbSession.DeleteAsync(_lookup, From, _keyMappingTarget, ct);
            }
        }
    }
}
