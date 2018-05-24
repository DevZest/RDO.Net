using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        public DbTableDelete<T> Delete(Func<T, _Boolean> where = null)
        {
            VerifyDeletable();
            return DbTableDelete<T>.Create(this, where);
        }

        internal DbSelectStatement BuildDeleteStatement(Func<T, _Boolean> where)
        {
            var whereExpr = VerifyWhere(where);
            return new DbSelectStatement(Model, null, null, whereExpr, null, -1, -1);
        }

        public DbTableDelete<T> Delete<TSource>(DbSet<TSource> source)
            where TSource : class, T, new()
        {
            return Delete(source, KeyMapping.Match);
        }

        public DbTableDelete<T> Delete<TSource>(DbSet<TSource> source, Func<TSource, T, KeyMapping> keyMapper)
            where TSource : class, IModelReference, new()
        {
            VerifyDeletable();
            Verify(source, nameof(source));
            var columnMappings = Verify(keyMapper, nameof(keyMapper), source._).GetColumnMappings();
            return DbTableDelete<T>.Create(this, source, columnMappings);
        }

        public DbTableDelete<T> Delete<TSource>(DataSet<TSource> source, int rowIndex)
            where TSource : class, T, new()
        {
            return Delete(source, rowIndex, KeyMapping.Match);
        }

        public DbTableDelete<T> Delete<TSource>(DataSet<TSource> source, int rowIndex, Func<TSource, T, KeyMapping> keyMapper)
            where TSource : class, IModelReference, new()
        {
            VerifyDeletable();
            Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = Verify(keyMapper, nameof(keyMapper), source._).GetColumnMappings();
            return DbTableDelete<T>.Create(this, source, rowIndex, columnMappings);
        }

        public DbTableDelete<T> Delete<TSource>(DataSet<TSource> source)
            where TSource : class, T, new()
        {
            return Delete(source, KeyMapping.Match);
        }

        public DbTableDelete<T> Delete<TSource>(DataSet<TSource> source, Func<TSource, T, KeyMapping> keyMapper)
            where TSource : class, IModelReference, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return Delete(source, 0, keyMapper);

            VerifyDeletable();
            var keyMappingTarget = Verify(keyMapper, nameof(keyMapper), source._).TargetKey;
            return DbTableDelete<T>.Create(this, source, keyMappingTarget);
        }

        internal DbSelectStatement BuildDeleteScalarStatement<TLookup>(DataSet<TLookup> source, int ordinal, IReadOnlyList<ColumnMapping> join)
            where TLookup : class, IModelReference, new()
        {
            Debug.Assert(source != null && source._ != null);
            return BuildDeleteScalarStatement(source[ordinal], join);
        }

        internal DbSelectStatement BuildDeleteScalarStatement(DataRow dataRow, IReadOnlyList<ColumnMapping> join)
        {
            var paramManager = new ScalarParamManager(dataRow);
            var from = new DbJoinClause(DbJoinKind.InnerJoin, GetScalarDataSource(paramManager, join), FromClause, join);
            return new DbSelectStatement(Model, null, from, null, null, -1, -1);
        }

        internal DbSelectStatement BuildDeleteStatement<TLookup>(DbSet<TLookup> source, IReadOnlyList<ColumnMapping> join)
            where TLookup : class, IModelReference, new()
        {
            Debug.Assert(source != null);
            Debug.Assert(join != null);
            return source.QueryStatement.BuildDeleteStatement(Model, join);
        }

        private void VerifyDeletable()
        {
            if (Model.ChildModels.Any(x => x != null))
                throw new NotSupportedException(DiagnosticMessages.DbTable_DeleteNotSupportedForParentTable);
        }
    }
}
