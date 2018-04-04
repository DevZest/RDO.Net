using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        public DbTableUpdate<T> Update(Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where = null)
        {
            var columnMappings = Verify(columnMapper, nameof(columnMapper));
            return DbTableUpdate<T>.Create(this, columnMappings, where);
        }

        internal DbSelectStatement BuildUpdateStatement(IReadOnlyList<ColumnMapping> columnMappings, Func<T, _Boolean> where)
        {
            Debug.Assert(columnMappings != null && columnMappings.Count > 0);
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, columnMappings, null, whereExpr, null, -1, -1);
        }

        public DbTableUpdate<T> Update<TSource>(DbSet<TSource> source)
            where TSource : T, new()
        {
            return Update(source, ColumnMapper.InferUpdate, KeyMapping.Infer);
        }

        public DbTableUpdate<T> Update<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> join)
            where TSource : Model, new()
        {
            Verify(source, nameof(source));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var keyMapping = Verify(join, nameof(join), source._);
            return DbTableUpdate<T>.Create(this, source, columnMappings, keyMapping.GetColumnMappings());
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, join);
        }

        internal DbSelectStatement BuildUpdateStatement<TSource>(DbSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            Debug.Assert(source != null);
            Debug.Assert(columnMapper != null);
            var columnMappings = Verify(columnMapper, source._);
            return source.QueryStatement.BuildUpdateStatement(Model, columnMappings, join);
        }

        public DbTableUpdate<T> Update<TSource>(DataSet<TSource> source, int rowIndex)
            where TSource : T, new()
        {
            return Update(source, rowIndex, ColumnMapper.InferUpdate, KeyMapping.Infer);
        }

        public DbTableUpdate<T> Update<TSource>(DataSet<TSource> source, int rowIndex, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper)
            where TSource : Model, new()
        {
            Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            var join = Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            return DbTableUpdate<T>.Create(this, source, rowIndex, columnMappings, join);
        }

        public DbTableUpdate<T> Update<TSource>(DataSet<TSource> source)
            where TSource : T, new()
        {
            return Update(source, ColumnMapper.InferUpdate, KeyMapping.Infer);
        }

        public DbTableUpdate<T> Update<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper)
            where TSource : Model, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return Update(source, 0, columnMapper, joinMapper);

            Verify(columnMapper, nameof(columnMapper));
            var joinTo = Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            return DbTableUpdate<T>.Create(this, source, columnMapper, joinTo);
        }

        internal DbSelectStatement BuildUpdateScalarStatement<TSource>(DataSet<TSource> dataSet, int ordinal, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : Model, new()
        {
            Debug.Assert(dataSet != null && dataSet._ != null);
            return BuildUpdateScalarStatement(dataSet[ordinal], columnMappings, join);
        }

        private DbSelectStatement BuildUpdateScalarStatement(DataRow dataRow, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var select = GetScalarMapping(paramManager, columnMappings);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, join), FromClause, join);
            return new DbSelectStatement(Model, select, from, null, null, -1, -1);
        }
    }
}
